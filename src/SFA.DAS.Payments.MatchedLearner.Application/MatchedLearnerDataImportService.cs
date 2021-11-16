using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerDataImportService
    {
        Task Import(SubmissionJobSucceeded submissionSucceededEvent);
    }

    public class MatchedLearnerDataImportService : IMatchedLearnerDataImportService
    {
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly IPaymentsRepository _paymentsRepository;

        public MatchedLearnerDataImportService(IMatchedLearnerRepository matchedLearnerRepository, IPaymentsRepository paymentsRepository)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
        }

        public async Task Import(SubmissionJobSucceeded submissionSucceededEvent)
        {
            var collectionPeriods = new List<byte> { submissionSucceededEvent.CollectionPeriod };

            if (submissionSucceededEvent.CollectionPeriod != 1)
            {
                collectionPeriods.Add((byte)(submissionSucceededEvent.CollectionPeriod - 1));
            }

            try
            {
                await _matchedLearnerRepository.BeginTransactionAsync(CancellationToken.None);

                await _matchedLearnerRepository.RemovePreviousSubmissionsData(submissionSucceededEvent.Ukprn, submissionSucceededEvent.AcademicYear, collectionPeriods);

                var dataLockEvents = await _paymentsRepository.GetDataLockEvents(submissionSucceededEvent);

                var apprenticeshipIds = dataLockEvents
                    .SelectMany(dle => dle.PayablePeriods)
                    .Select(dlepp => dlepp.ApprenticeshipId ?? 0)
                    .Union(dataLockEvents.SelectMany(dle => dle.NonPayablePeriods).SelectMany(dlenpp => dlenpp.Failures)
                        .Select(dlenppf => dlenppf.ApprenticeshipId ?? 0))
                    .ToList();

                var apprenticeships = await _paymentsRepository.GetApprenticeships(apprenticeshipIds);

                await _matchedLearnerRepository.RemoveApprenticeships(apprenticeshipIds);

                await _matchedLearnerRepository.StoreApprenticeships(apprenticeships, CancellationToken.None);

                await _matchedLearnerRepository.StoreDataLocks(dataLockEvents, CancellationToken.None);

                await _matchedLearnerRepository.CommitTransactionAsync(CancellationToken.None);
            }
            catch
            {
               await _matchedLearnerRepository.RollbackTransactionAsync(CancellationToken.None);
               throw;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface ILegacyMatchedLearnerDataImportService
    {
        Task Import(SubmissionJobSucceeded submissionSucceededEvent, List<DataLockEventModel> dataLockEvents);
    }

    public class LegacyMatchedLearnerDataImportService : ILegacyMatchedLearnerDataImportService
    {
        private readonly ILegacyMatchedLearnerRepository _legacyMatchedLearnerRepository;
        private readonly IPaymentsRepository _paymentsRepository;

        public LegacyMatchedLearnerDataImportService(ILegacyMatchedLearnerRepository legacyMatchedLearnerRepository, IPaymentsRepository paymentsRepository)
        {
            _legacyMatchedLearnerRepository = legacyMatchedLearnerRepository ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
        }

        public async Task Import(SubmissionJobSucceeded submissionSucceededEvent, List<DataLockEventModel> dataLockEvents)
        {
            var collectionPeriods = new List<byte> { submissionSucceededEvent.CollectionPeriod };

            if (submissionSucceededEvent.CollectionPeriod != 1)
            {
                collectionPeriods.Add((byte)(submissionSucceededEvent.CollectionPeriod - 1));
            }

            try
            {
                await _legacyMatchedLearnerRepository.BeginTransactionAsync(CancellationToken.None);

                await _legacyMatchedLearnerRepository.RemovePreviousSubmissionsData(submissionSucceededEvent.Ukprn, submissionSucceededEvent.AcademicYear, collectionPeriods);

                var apprenticeshipIds = dataLockEvents
                    .SelectMany(dle => dle.PayablePeriods)
                    .Select(dlepp => dlepp.ApprenticeshipId ?? 0)
                    .Union(dataLockEvents.SelectMany(dle => dle.NonPayablePeriods).SelectMany(dlenpp => dlenpp.Failures)
                        .Select(dlenppf => dlenppf.ApprenticeshipId ?? 0))
                    .ToList();

                var apprenticeships = await _paymentsRepository.GetApprenticeships(apprenticeshipIds);

                await _legacyMatchedLearnerRepository.RemoveApprenticeships(apprenticeshipIds);

                await _legacyMatchedLearnerRepository.StoreApprenticeships(apprenticeships, CancellationToken.None);

                await _legacyMatchedLearnerRepository.StoreDataLocks(dataLockEvents, CancellationToken.None);

                await _legacyMatchedLearnerRepository.CommitTransactionAsync(CancellationToken.None);
            }
            catch
            {
               await _legacyMatchedLearnerRepository.RollbackTransactionAsync(CancellationToken.None);
               throw;
            }
        }
    }
}
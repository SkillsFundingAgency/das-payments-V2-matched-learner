using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerDataImportService
    {
        Task Import(SubmissionJobSucceeded submissionSucceededEvent, List<DataLockEventModel> dataLockEvents);
    }

    public class MatchedLearnerDataImportService : IMatchedLearnerDataImportService
    {
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IMatchedLearnerDtoMapper _matchedLearnerDtoMapper;

        public MatchedLearnerDataImportService(IMatchedLearnerRepository matchedLearnerRepository, IPaymentsRepository  paymentsRepository, IMatchedLearnerDtoMapper matchedLearnerDtoMapper)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper ?? throw new ArgumentNullException(nameof(matchedLearnerDtoMapper));
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
                await _matchedLearnerRepository.BeginTransactionAsync(CancellationToken.None);

                await _matchedLearnerRepository.RemovePreviousSubmissionsData(submissionSucceededEvent.Ukprn, submissionSucceededEvent.AcademicYear, collectionPeriods);

                var apprenticeships = new List<ApprenticeshipModel>();

                //var apprenticeshipIds = dataLockEventPayablePeriods.Select(d => d.ApprenticeshipId)
                //    .Union(dataLockEventNonPayablePeriodFailures.Select(d => d.ApprenticeshipId))
                //    .Distinct()
                //    .ToList();

                //var apprenticeshipDetails = new List<ApprenticeshipModel>();
                //if (apprenticeshipIds.Any())
                //{
                //    apprenticeshipDetails = await _dataContext.Apprenticeship.Where(a => apprenticeshipIds.Contains(a.Id)).ToListAsync();
                //}

                var trainings = _matchedLearnerDtoMapper.MapToModel(dataLockEvents, apprenticeships);

                await _matchedLearnerRepository.StoreSubmissionsData(trainings, CancellationToken.None);

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
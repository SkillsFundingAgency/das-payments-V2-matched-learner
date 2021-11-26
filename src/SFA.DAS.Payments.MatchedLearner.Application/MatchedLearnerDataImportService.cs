using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data;
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
        private readonly ILogger<MatchedLearnerDataImportService> _logger;

        public MatchedLearnerDataImportService(IMatchedLearnerRepository matchedLearnerRepository, IPaymentsRepository paymentsRepository, IMatchedLearnerDtoMapper matchedLearnerDtoMapper, ILogger<MatchedLearnerDataImportService> logger)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper ?? throw new ArgumentNullException(nameof(matchedLearnerDtoMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                var apprenticeshipIds =
                    dataLockEvents.SelectMany(d => d.PayablePeriods).Select(a => a.ApprenticeshipId ?? 0).Union(
                    dataLockEvents.SelectMany(d => d.NonPayablePeriods)
                        .SelectMany(d => d.Failures).Select(f => f.ApprenticeshipId ?? 0))
                    .Distinct()
                    .ToList();

                var apprenticeshipDetails = new List<ApprenticeshipModel>();
                if (apprenticeshipIds.Any())
                {
                    apprenticeshipDetails = await _paymentsRepository.GetApprenticeships(apprenticeshipIds);
                }

                await _matchedLearnerRepository.BeginTransactionAsync();

                await _matchedLearnerRepository.RemovePreviousSubmissionsData(submissionSucceededEvent.Ukprn, submissionSucceededEvent.AcademicYear, collectionPeriods);


                var trainings = _matchedLearnerDtoMapper.MapToModel(dataLockEvents, apprenticeshipDetails);


                try
                {
                    await _matchedLearnerRepository.SaveTrainings(trainings.Clone());
                }
                catch (Exception e)
                {
                    if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;

                    _logger.LogWarning($"Batch contained a duplicate DataLock.  Will store each individually and discard duplicate. Inner exception {e}");

                    await _matchedLearnerRepository.SaveTrainingsIndividually(trainings);
                }

                await _matchedLearnerRepository.CommitTransactionAsync();
            }
            catch(Exception exception)
            {
                await _matchedLearnerRepository.RollbackTransactionAsync();

                _logger.LogError(exception,$"Error Importing Training Data. JobId: {submissionSucceededEvent.JobId}, AcademicYear: {submissionSucceededEvent.AcademicYear}, CollectionPeriod: {submissionSucceededEvent.CollectionPeriod}");

                throw;
            }
        }
    }
}
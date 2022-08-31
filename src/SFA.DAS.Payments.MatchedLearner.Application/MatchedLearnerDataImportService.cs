using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerDataImportService
    {
        Task Import(ImportMatchedLearnerData importMatchedLearnerData);
    }

    public class MatchedLearnerDataImportService : IMatchedLearnerDataImportService
    {
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly ILogger<MatchedLearnerDataImportService> _logger;

        public MatchedLearnerDataImportService(IMatchedLearnerRepository matchedLearnerRepository, IPaymentsRepository paymentsRepository, ILogger<MatchedLearnerDataImportService> logger)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Import(ImportMatchedLearnerData importMatchedLearnerData)
        {
            _logger.LogInformation($"Started MatchedLearner Data Import for ukprn {importMatchedLearnerData.Ukprn}");

            try
            {
                await _matchedLearnerRepository.BeginTransactionAsync(CancellationToken.None);

                await _matchedLearnerRepository.RemovePreviousSubmissionsData(importMatchedLearnerData.Ukprn, importMatchedLearnerData.AcademicYear, importMatchedLearnerData.CollectionPeriod);

                var dataLockEvents = await _paymentsRepository.GetDataLockEvents(importMatchedLearnerData);

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

                await _matchedLearnerRepository.SaveSubmissionJob(new SubmissionJobModel
                {
                    CollectionPeriod = importMatchedLearnerData.CollectionPeriod,
                    DcJobId = importMatchedLearnerData.JobId,
                    Ukprn = importMatchedLearnerData.Ukprn,
                    AcademicYear = importMatchedLearnerData.AcademicYear,
                    IlrSubmissionDateTime = importMatchedLearnerData.IlrSubmissionDateTime,
                    EventTime = importMatchedLearnerData.EventTime
                });

                await _matchedLearnerRepository.CommitTransactionAsync(CancellationToken.None);

                _logger.LogInformation($"Finished MatchedLearner Data Import for ukprn {importMatchedLearnerData.Ukprn}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in MatchedLearner Data Import for ukprn {importMatchedLearnerData.Ukprn}, Inner Exception {exception}");

               await _matchedLearnerRepository.RollbackTransactionAsync(CancellationToken.None);
               throw;
            }
        }
    }
}
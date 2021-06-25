using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application.Data;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Repositories
{
    public interface IPaymentsDataLockRepository
    {
        Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln);
    }

    public class PaymentsDataLockRepository : IPaymentsDataLockRepository
    {
        private readonly IPaymentsContext _context;
        private readonly ILogger<PaymentsDataLockRepository> _logger;

        public PaymentsDataLockRepository(IPaymentsContext context, ILogger<PaymentsDataLockRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new MatchedLearnerDataLockInfo
            {
                DataLockEvents = new List<DataLockEvent>(),
                DataLockEventPriceEpisodes = new List<DataLockEventPriceEpisode>(),
                DataLockEventPayablePeriods = new List<DataLockEventPayablePeriod>(),
                DataLockEventNonPayablePeriods = new List<DataLockEventNonPayablePeriod>(),
                DataLockEventNonPayablePeriodFailures = new List<DataLockEventNonPayablePeriodFailure>(),
                Apprenticeships = new List<Apprenticeship>()
            };

            var academicYears = _context.LatestSuccessfulJobs
                .Where(x => x.Ukprn == ukprn)
                .Select(x => x.AcademicYear)
                .OrderByDescending(x => x)
                .Distinct()
                .ToList();

            foreach (var academicYear in academicYears)
            {
                var latestSuccessfulJob = _context.LatestSuccessfulJobs
                    .Where(y => y.Ukprn == ukprn)
                    .Where(y => y.AcademicYear == academicYear)
                    .OrderByDescending(y => y.CollectionPeriod)
                    .First();

                _logger.LogDebug($"Getting DataLock Event Data Uln: {uln}, Academic year: {latestSuccessfulJob.AcademicYear}, Collection period: {latestSuccessfulJob.CollectionPeriod}");

                var transactionTypes = new List<byte> { 1, 2, 3 };

                var dataLockEvents = await _context.DataLockEvent
                    .Where(x =>
                        x.LearningAimReference == "ZPROG001" &&
                        x.Ukprn == ukprn &&
                        x.LearnerUln == uln &&
                        x.JobId == latestSuccessfulJob.DcJobId &&
                        x.AcademicYear == latestSuccessfulJob.AcademicYear &&
                        x.CollectionPeriod == latestSuccessfulJob.CollectionPeriod)
                    .OrderBy(x => x.LearningStartDate)
                    .ToListAsync();

                var eventIds = dataLockEvents.Select(d => d.EventId).ToList();

                var dataLockEventPriceEpisodes = await _context.DataLockEventPriceEpisode
                    .Where(d => eventIds.Contains(d.DataLockEventId) && d.PriceEpisodeIdentifier != null)
                    .OrderBy(p => p.StartDate)
                    .ToListAsync();

                var dataLockEventPayablePeriods = await _context.DataLockEventPayablePeriod
                    .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                    .OrderBy(p => p.DeliveryPeriod)
                    .ToListAsync();

                var dataLockEventNonPayablePeriods = await _context.DataLockEventNonPayablePeriod
                    .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                    .OrderBy(p => p.DeliveryPeriod)
                    .ToListAsync();

                var dataLockEventNonPayablePeriodIds = dataLockEventNonPayablePeriods.Select(d => d.DataLockEventNonPayablePeriodId).ToList();

                var dataLockEventNonPayablePeriodFailures = await _context.DataLockEventNonPayablePeriodFailures
                    .Where(d => dataLockEventNonPayablePeriodIds.Contains(d.DataLockEventNonPayablePeriodId))
                    .ToListAsync();

                var apprenticeshipIds = dataLockEventPayablePeriods.Select(d => d.ApprenticeshipId)
                     .Union(dataLockEventNonPayablePeriodFailures.Select(d => d.ApprenticeshipId))
                     .Distinct()
                     .ToList();

                var apprenticeshipDetails = await _context.Apprenticeship.Where(a => apprenticeshipIds.Contains(a.Id)).ToListAsync();

                result.DataLockEvents.AddRange(dataLockEvents);
                result.DataLockEventPriceEpisodes.AddRange(dataLockEventPriceEpisodes);
                result.DataLockEventPayablePeriods.AddRange(dataLockEventPayablePeriods);
                result.DataLockEventNonPayablePeriods.AddRange(dataLockEventNonPayablePeriods);
                result.DataLockEventNonPayablePeriodFailures.AddRange(dataLockEventNonPayablePeriodFailures);
                result.Apprenticeships.AddRange(apprenticeshipDetails);
            }

            stopwatch.Stop();

            _logger.LogInformation($"Finished getting DataLock Event Data Duration: {stopwatch.ElapsedMilliseconds} Uln: {uln}, Academic years covered: {string.Join(",", academicYears)}");

            return result;
        }
    }
}

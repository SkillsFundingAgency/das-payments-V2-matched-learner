using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application.Data;

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

            var latestSuccessfulJob = await _context.LatestSuccessfulJobs
                .Where(y => y.Ukprn == ukprn)
                .OrderByDescending(y => y.AcademicYear)
                .ThenByDescending(y => y.CollectionPeriod)
                .FirstOrDefaultAsync();

            if (latestSuccessfulJob == null)
            {
                stopwatch.Stop();
                _logger.LogInformation($"No Data in current academic year CollectionPeriod for Uln: {uln}, Duration: {stopwatch.ElapsedMilliseconds}");
                return new MatchedLearnerDataLockInfo();
            }

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

            //todo order by academic year

            //todo this should potentially be dataLockEvents for the latestSuccessfulJob from the previous academic year? //answer - all academic years, latest for each
            //if so, how to work out this job? //remove academicYear filter
            var dataLockEventsForAllPeriods = await _context.DataLockEvent
                .Where(x =>
                    x.LearningAimReference == "ZPROG001" &&
                    x.Ukprn == ukprn &&
                    x.LearnerUln == uln)
                .OrderBy(x => x.LearningStartDate)
                .ToListAsync();

            var eventIds = dataLockEvents.Select(d => d.EventId).ToList();
            var eventIdsForAllPeriods = dataLockEventsForAllPeriods.Select(d => d.EventId).ToList();

            //todo update this to pull all price episodes for the learning aim
            var dataLockEventPriceEpisodes = await _context.DataLockEventPriceEpisode
                .Where(d => eventIdsForAllPeriods.Contains(d.DataLockEventId) && d.PriceEpisodeIdentifier != null)
                .OrderBy(p => p.StartDate)
                .ToListAsync();

            //todo do these need updating to or do we only care about price episodes? //they all need updating
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

            stopwatch.Stop();

            _logger.LogInformation($"Finished getting DataLock Event Data Duration: {stopwatch.ElapsedMilliseconds} Uln: {uln}, Academic year: {latestSuccessfulJob.AcademicYear}, Collection period: {latestSuccessfulJob.CollectionPeriod}");

            return new MatchedLearnerDataLockInfo
            {
                DataLockEvents = dataLockEvents,
                DataLockEventPriceEpisodes = dataLockEventPriceEpisodes,
                DataLockEventPayablePeriods = dataLockEventPayablePeriods,
                DataLockEventNonPayablePeriods = dataLockEventNonPayablePeriods,
                DataLockEventNonPayablePeriodFailures = dataLockEventNonPayablePeriodFailures,
                Apprenticeships = apprenticeshipDetails
            };
        }
    }
}

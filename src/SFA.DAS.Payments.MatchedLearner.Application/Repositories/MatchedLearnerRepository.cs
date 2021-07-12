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
    public interface IMatchedLearnerRepository
    {
        Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln);
    }

    public class MatchedLearnerRepository : IMatchedLearnerRepository
    {
        private readonly IMatchedLearnerContext _context;
        private readonly ILogger<MatchedLearnerRepository> _logger;

        public MatchedLearnerRepository(IMatchedLearnerContext context, ILogger<MatchedLearnerRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln)
        {
            var stopwatch = Stopwatch.StartNew();

            var transactionTypes = new List<byte> { 1, 2, 3 };

            var dataLockEvents = await _context.DataLockEvent
                .Where(x =>
                    x.LearningAimReference == "ZPROG001" &&
                    x.Ukprn == ukprn &&
                    x.LearnerUln == uln)
                .OrderBy(x => x.LearningStartDate)
                .ToListAsync();
            
            if (dataLockEvents == null)
            {
                stopwatch.Stop();
                _logger.LogInformation($"No Data for Uln: {uln}, Duration: {stopwatch.ElapsedMilliseconds}");
                return new MatchedLearnerDataLockInfo();
            }

            _logger.LogDebug($"Getting DataLock Event Data Uln: {uln}");

            var eventId = dataLockEvents.Select(d => d.EventId).ToList();

            var dataLockEventPriceEpisodes = await _context.DataLockEventPriceEpisode
                .Where(d => eventId.Contains(d.DataLockEventId) && d.PriceEpisodeIdentifier != null)
                .OrderBy(p => p.StartDate)
                .ToListAsync();


            var dataLockEventPayablePeriods = await _context.DataLockEventPayablePeriod
                .Where(d => eventId.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                .OrderBy(p => p.DeliveryPeriod)
                .ToListAsync();

            var dataLockEventNonPayablePeriods = await _context.DataLockEventNonPayablePeriod
                .Where(d => eventId.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
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

            _logger.LogInformation($"Finished getting DataLock Event Data Duration: {stopwatch.ElapsedMilliseconds} Uln: {uln}");

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

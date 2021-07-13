using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore.Storage;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IMatchedLearnerRepository
    {
        Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln);
        Task RemovePreviousSubmissionData(long ukprn, short academicYear, IList<byte> collectionPeriod);
        Task StoreDataLocks(List<DataLockEventModel> models, CancellationToken cancellationToken);
    }

    public class MatchedLearnerRepository : IMatchedLearnerRepository
    {
        private readonly IMatchedLearnerContext _context;
        private readonly IMatchedLearnerDataContextFactory _retryDataContextFactory;

        private readonly ILogger<MatchedLearnerRepository> _logger;

        public MatchedLearnerRepository(IMatchedLearnerContext context, IMatchedLearnerDataContextFactory  matchedLearnerDataContextFactory, ILogger<MatchedLearnerRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _retryDataContextFactory = matchedLearnerDataContextFactory ?? throw new ArgumentNullException(nameof(matchedLearnerDataContextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task SaveDataLockEvents(List<DataLockEventModel> dataLockEvents, CancellationToken cancellationToken)
        {
            using (var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false))
            {
                var bulkConfig = new BulkConfig { SetOutputIdentity = false, BulkCopyTimeout = 60, PreserveInsertOrder = false };
                var priceEpisodes = dataLockEvents
                    .SelectMany(dataLockEvent => dataLockEvent.PriceEpisodes)
                    .ToList();
                var payablePeriods = dataLockEvents
                    .SelectMany(dataLockEvent => dataLockEvent.PayablePeriods)
                    .ToList();
                var nonPayablePeriods = dataLockEvents
                    .SelectMany(dataLockEvent => dataLockEvent.NonPayablePeriods)
                    .ToList();
                var failures = dataLockEvents
                    .SelectMany(dataLockEvent => dataLockEvent.NonPayablePeriods
                        .SelectMany(npp => npp.Failures))
                    .ToList();

                await ((DbContext)_context).BulkInsertAsync(dataLockEvents, bulkConfig, null, cancellationToken)
                    .ConfigureAwait(false);
                await ((DbContext)_context).BulkInsertAsync(priceEpisodes, bulkConfig, null, cancellationToken)
                    .ConfigureAwait(false);
                await ((DbContext)_context).BulkInsertAsync(payablePeriods, bulkConfig, null, cancellationToken)
                    .ConfigureAwait(false);
                await ((DbContext)_context).BulkInsertAsync(nonPayablePeriods, bulkConfig, null, cancellationToken)
                    .ConfigureAwait(false);
                await ((DbContext)_context).BulkInsertAsync(failures, bulkConfig, null, cancellationToken)
                    .ConfigureAwait(false);
                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SaveDataLocksIndividually(List<DataLockEventModel> dataLockEvents, CancellationToken cancellationToken)
        {
            var mainContext = _retryDataContextFactory.Create();

            using (var tx = await ((DbContext)mainContext).Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false))
            {
                foreach (var dataLockEvent in dataLockEvents)
                {
                    try
                    {
                        var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                        await retryDataContext.DataLockEvent.AddAsync(dataLockEvent, cancellationToken).ConfigureAwait(false);
                        await retryDataContext.SaveChanges(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        if (e.IsUniqueKeyConstraintException())
                        {
                            _logger.LogInformation($"Discarding duplicate DataLock event. Event Id: {dataLockEvent.EventId}, JobId: {dataLockEvent.JobId}, Learn ref: {dataLockEvent.LearnerReferenceNumber},  Event Type: {dataLockEvent.EventType}");
                            continue;
                        }
                        throw;
                    }
                }
                await tx.CommitAsync(cancellationToken);
            }
        }

        public async Task StoreDataLocks(List<DataLockEventModel> models, CancellationToken cancellationToken)
        {
            try
            {
                await SaveDataLockEvents(models, cancellationToken);
            }
            catch (Exception e)
            {
                if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;

                _logger.LogInformation($"Batch contained a duplicate DataLock.  Will store each individually and discard duplicate.");

                await SaveDataLocksIndividually(models, cancellationToken).ConfigureAwait(false);
            }
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

        public async Task RemovePreviousSubmissionData(long ukprn, short academicYear, IList<byte> collectionPeriod)
        {
            await _context.DeleteLearnerData(ukprn, academicYear, collectionPeriod);
        }
    }
}
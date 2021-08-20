using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IMatchedLearnerRepository
    {
        Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln);
        Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod);
        Task StoreApprenticeships(List<ApprenticeshipModel> apprenticeships, CancellationToken cancellationToken);
        Task StoreDataLocks(List<DataLockEventModel> models, CancellationToken cancellationToken);
        Task RemoveApprenticeships(List<long> apprenticeshipIds);
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
    }

    public class MatchedLearnerRepository : IMatchedLearnerRepository
    {
        private readonly MatchedLearnerDataContext _dataContext;
        private readonly ILogger<MatchedLearnerRepository> _logger;
        private readonly IMatchedLearnerDataContextFactory _retryDataContextFactory;
        private IDbContextTransaction _transaction;

        public MatchedLearnerRepository(MatchedLearnerDataContext dataContext, IMatchedLearnerDataContextFactory matchedLearnerDataContextFactory, ILogger<MatchedLearnerRepository> logger)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _retryDataContextFactory = matchedLearnerDataContextFactory ?? throw new ArgumentNullException(nameof(matchedLearnerDataContextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            _transaction = await _dataContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln)
        {
            var stopwatch = Stopwatch.StartNew();

            var transactionTypes = new List<byte> { 1, 2, 3 };

            var dataLockEvents = await _dataContext.DataLockEvent
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

            var eventIds = dataLockEvents.Select(d => d.EventId).ToList();

            var dataLockEventPriceEpisodes = await _dataContext.DataLockEventPriceEpisode
                .Where(d => eventIds.Contains(d.DataLockEventId) && d.PriceEpisodeIdentifier != null)
                .OrderBy(p => p.StartDate)
                .ThenBy(p => p.PriceEpisodeIdentifier)
                .ToListAsync();

            var dataLockEventPayablePeriods = await _dataContext.DataLockEventPayablePeriod
                .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                .OrderBy(p => p.DeliveryPeriod)
                .ToListAsync();

            var dataLockEventNonPayablePeriods = await _dataContext.DataLockEventNonPayablePeriod
                .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                .OrderBy(p => p.DeliveryPeriod)
                .ToListAsync();

            var dataLockEventNonPayablePeriodIds = dataLockEventNonPayablePeriods.Select(d => d.DataLockEventNonPayablePeriodId).ToList();

            var dataLockEventNonPayablePeriodFailures = new List<DataLockEventNonPayablePeriodFailureModel>();
            if (dataLockEventNonPayablePeriodIds.Any())
            {
                dataLockEventNonPayablePeriodFailures = await _dataContext.DataLockEventNonPayablePeriodFailures
                .Where(d => dataLockEventNonPayablePeriodIds.Contains(d.DataLockEventNonPayablePeriodId))
                .ToListAsync();
            }

            var apprenticeshipIds = dataLockEventPayablePeriods.Select(d => d.ApprenticeshipId)
                 .Union(dataLockEventNonPayablePeriodFailures.Select(d => d.ApprenticeshipId))
                 .Distinct()
                 .ToList();

            var apprenticeshipDetails = new List<ApprenticeshipModel>();
            if (apprenticeshipIds.Any())
            {
                apprenticeshipDetails = await _dataContext.Apprenticeship.Where(a => apprenticeshipIds.Contains(a.Id)).ToListAsync();
            }

            _logger.LogInformation($"Finished getting DataLock Event Data Duration: {stopwatch.ElapsedMilliseconds} Uln: {uln}");

            var result = new MatchedLearnerDataLockInfo
            {
                DataLockEvents = dataLockEvents,
                DataLockEventPriceEpisodes = dataLockEventPriceEpisodes,
                DataLockEventPayablePeriods = dataLockEventPayablePeriods,
                DataLockEventNonPayablePeriods = dataLockEventNonPayablePeriods,
                DataLockEventNonPayablePeriodFailures = dataLockEventNonPayablePeriodFailures,
                Apprenticeships = apprenticeshipDetails
            };

            stopwatch.Stop();

            _logger.LogInformation($"Finished getting DataLock Event Data for Uln: {uln}, Duration: {stopwatch.ElapsedMilliseconds}");

            return result;
        }



        public async Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod)
        {
            await _dataContext.RemovePreviousSubmissionsData(ukprn, academicYear, collectionPeriod);
        }

        public async Task RemoveApprenticeships(List<long> apprenticeshipIds)
        {
            var apprenticeshipBatches = apprenticeshipIds.Batch(2000);

            foreach (var batch in apprenticeshipBatches)
            {
                await _dataContext.RemoveApprenticeships(batch);
            }
        }



        public async Task SaveApprenticeships(List<ApprenticeshipModel> apprenticeships, CancellationToken cancellationToken)
        {
            var bulkConfig = new BulkConfig { SetOutputIdentity = false, BulkCopyTimeout = 60, PreserveInsertOrder = false };

            await _dataContext.BulkInsertAsync(apprenticeships, bulkConfig, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task StoreApprenticeships(List<ApprenticeshipModel> models, CancellationToken cancellationToken)
        {
            try
            {
                await SaveApprenticeships(models, cancellationToken);
            }
            catch (Exception e)
            {
                if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;

                _logger.LogInformation("Batch contained a duplicate DataLock.  Will store each individually and discard duplicate.");

                await SaveApprenticeshipsIndividually(models, cancellationToken).ConfigureAwait(false);
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

                _logger.LogInformation("Batch contained a duplicate DataLock.  Will store each individually and discard duplicate.");

                await SaveDataLocksIndividually(models, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SaveApprenticeshipsIndividually(List<ApprenticeshipModel> apprenticeships, CancellationToken cancellationToken)
        {
            var mainContext = _retryDataContextFactory.Create();

            await using var tx = await ((DbContext)mainContext).Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false);

            foreach (var apprenticeship in apprenticeships)
            {
                try
                {
                    var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                    await retryDataContext.Apprenticeship.AddAsync(apprenticeship, cancellationToken).ConfigureAwait(false);
                    await retryDataContext.SaveChanges(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (!e.IsUniqueKeyConstraintException()) throw;

                    _logger.LogInformation($"Discarding duplicate apprenticeship. Id: {apprenticeship.Id}, ukprn: {apprenticeship.Ukprn}");
                }
            }

            await tx.CommitAsync(cancellationToken);
        }

        private async Task SaveDataLockEvents(IList<DataLockEventModel> dataLockEvents, CancellationToken cancellationToken)
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

            await _dataContext.BulkInsertAsync(dataLockEvents, bulkConfig, null, cancellationToken)
                .ConfigureAwait(false);
            await _dataContext.BulkInsertAsync(priceEpisodes, bulkConfig, null, cancellationToken)
                .ConfigureAwait(false);
            await _dataContext.BulkInsertAsync(payablePeriods, bulkConfig, null, cancellationToken)
                .ConfigureAwait(false);
            await _dataContext.BulkInsertAsync(nonPayablePeriods, bulkConfig, null, cancellationToken)
                .ConfigureAwait(false);
            await _dataContext.BulkInsertAsync(failures, bulkConfig, null, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SaveDataLocksIndividually(List<DataLockEventModel> dataLockEvents, CancellationToken cancellationToken)
        {
            var mainContext = _retryDataContextFactory.Create();

            await using var tx = await ((DbContext)mainContext).Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false);

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
}
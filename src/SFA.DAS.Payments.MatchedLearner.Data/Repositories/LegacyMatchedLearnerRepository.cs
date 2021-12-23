using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
    public interface ILegacyMatchedLearnerRepository
    {
        Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod);
        Task StoreApprenticeships(List<ApprenticeshipModel> apprenticeships);
        Task RemoveApprenticeships(List<long> apprenticeshipIds);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<MatchedLearnerDataLockInfo> GetDataLockEvents(long ukprn, long uln);
        Task SaveDataLockEvents(IList<DataLockEventModel> dataLockEvents);
        Task SaveDataLocksIndividually(List<DataLockEventModel> dataLockEvents);
    }

    public class LegacyMatchedLearnerRepository : ILegacyMatchedLearnerRepository
    {
        private readonly MatchedLearnerDataContext _dataContext;
        private readonly ILogger<MatchedLearnerRepository> _logger;
        private readonly IMatchedLearnerDataContextFactory _retryDataContextFactory;
        private IDbContextTransaction _transaction;

        public LegacyMatchedLearnerRepository(MatchedLearnerDataContext dataContext, IMatchedLearnerDataContextFactory matchedLearnerDataContextFactory, ILogger<MatchedLearnerRepository> logger)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _retryDataContextFactory = matchedLearnerDataContextFactory ?? throw new ArgumentNullException(nameof(matchedLearnerDataContextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _dataContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted);
        }

        public async Task CommitTransactionAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
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

            _logger.LogInformation($"Started Getting DataLock Event Data from database for Uln: {uln}");

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

            _logger.LogInformation($"Finished Getting DataLock Event Data from database for Uln: {uln}, Duration: {stopwatch.ElapsedMilliseconds}");

            return result;
        }

        public async Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod)
        {
            var sqlParameters = collectionPeriod.Select((item, index) => new SqlParameter($"@period{index}", item)).ToList();
            var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));

            sqlParameters.Add(new SqlParameter("@ukprn", ukprn));
            sqlParameters.Add(new SqlParameter("@academicYear", academicYear));

            await _dataContext.Database.ExecuteSqlRawAsync($"DELETE FROM Payments2.DataLockEvent WHERE ukprn = @ukprn AND AcademicYear = @academicYear AND CollectionPeriod IN ({ sqlParamName })", sqlParameters);
        }

        public async Task RemoveApprenticeships(List<long> apprenticeshipIds)
        {
            var apprenticeshipBatches = apprenticeshipIds.Batch(2000);

            foreach (var batch in apprenticeshipBatches)
            {
                var sqlParameters = batch.Select((item, index) => new SqlParameter($"@Id{index}", item)).ToList();
                var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));

                await _dataContext.Database.ExecuteSqlRawAsync($"DELETE FROM Payments2.Apprenticeship WHERE id IN ( {sqlParamName} )", sqlParameters);
            }
        }

        public async Task SaveApprenticeships(List<ApprenticeshipModel> apprenticeships)
        {
            var bulkConfig = new BulkConfig { SetOutputIdentity = false, BulkCopyTimeout = 60, PreserveInsertOrder = false };

            await _dataContext.BulkInsertAsync(apprenticeships, bulkConfig);
        }

        public async Task StoreApprenticeships(List<ApprenticeshipModel> models)
        {
            try
            {
                await SaveApprenticeships(models);
            }
            catch (Exception e)
            {
                if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;

                _logger.LogInformation("Batch contained a duplicate DataLock.  Will store each individually and discard duplicate.");

                await SaveApprenticeshipsIndividually(models);
            }
        }

        private async Task SaveApprenticeshipsIndividually(List<ApprenticeshipModel> apprenticeships)
        {
            var mainContext = _retryDataContextFactory.Create();

            await using var tx = await mainContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted);

            foreach (var apprenticeship in apprenticeships)
            {
                try
                {
                    var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                    await retryDataContext.Apprenticeship.AddAsync(apprenticeship);
                    await retryDataContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    if (!e.IsUniqueKeyConstraintException()) throw;

                    _logger.LogInformation($"Discarding duplicate apprenticeship. Id: {apprenticeship.Id}, ukprn: {apprenticeship.Ukprn}");
                }
            }

            await tx.CommitAsync();
        }

        public async Task SaveDataLockEvents(IList<DataLockEventModel> dataLockEvents)
        {
            var bulkConfig = new BulkConfig { SetOutputIdentity = false, BulkCopyTimeout = 150, PreserveInsertOrder = false, UseTempDB = true };

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

            await _dataContext.BulkInsertAsync(dataLockEvents, bulkConfig.Clone());
            await _dataContext.BulkInsertAsync(priceEpisodes, bulkConfig.Clone());
            await _dataContext.BulkInsertAsync(payablePeriods, bulkConfig.Clone());
            await _dataContext.BulkInsertAsync(nonPayablePeriods, bulkConfig.Clone());
            await _dataContext.BulkInsertAsync(failures, bulkConfig);
        }

        public async Task SaveDataLocksIndividually(List<DataLockEventModel> dataLockEvents)
        {
            var mainContext = _retryDataContextFactory.Create();

            await using var tx = await mainContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted);

            foreach (var dataLockEvent in dataLockEvents)
            {
                try
                {
                    var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                    await retryDataContext.DataLockEvent.AddAsync(dataLockEvent);
                    await retryDataContext.SaveChangesAsync();
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

            await tx.CommitAsync();
        }
    }
}
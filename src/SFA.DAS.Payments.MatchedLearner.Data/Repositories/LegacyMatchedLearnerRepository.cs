using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
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
        Task StoreApprenticeships(List<ApprenticeshipModel> apprenticeships, CancellationToken cancellationToken);
        Task StoreDataLocks(List<DataLockEventModel> models, CancellationToken cancellationToken);
        Task RemoveApprenticeships(List<long> apprenticeshipIds);
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
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

            await using var tx = await mainContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false);

            foreach (var apprenticeship in apprenticeships)
            {
                try
                {
                    var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                    await retryDataContext.Apprenticeship.AddAsync(apprenticeship, cancellationToken).ConfigureAwait(false);
                    await retryDataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

            await using var tx = await mainContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false);

            foreach (var dataLockEvent in dataLockEvents)
            {
                try
                {
                    var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                    await retryDataContext.DataLockEvent.AddAsync(dataLockEvent, cancellationToken).ConfigureAwait(false);
                    await retryDataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
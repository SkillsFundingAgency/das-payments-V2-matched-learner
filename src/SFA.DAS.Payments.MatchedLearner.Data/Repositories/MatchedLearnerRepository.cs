using System;
using System.Collections.Generic;
using System.Data;
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
        Task<List<TrainingModel>> GetMatchedLearnerTrainings(long ukprn, long uln);
        Task<List<DataLockEventModel>> GetDataLockEventsForMigration(long ukprn);
        Task<List<ApprenticeshipModel>> GetApprenticeshipsForMigration(List<long> apprenticeshipIds);
        Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod);
        Task StoreSubmissionsData(List<TrainingModel> trainings, CancellationToken cancellationToken);
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
        Task SaveTrainingsIndividually(List<TrainingModel> models, CancellationToken cancellationToken);
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

        //TODO: implement this method from new table structure
        public Task<List<TrainingModel>> GetMatchedLearnerTrainings(long ukprn, long uln)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DataLockEventModel>> GetDataLockEventsForMigration(long ukprn)
        {
            return await _dataContext.DataLockEvent
                .Include(d => d.NonPayablePeriods)
                .ThenInclude(npp => npp.Failures)
                .Include(d => d.PayablePeriods)
                .Include(d => d.PriceEpisodes)
                .Where(d => d.Ukprn == ukprn)
                .ToListAsync();
        }

        public async Task<List<ApprenticeshipModel>> GetApprenticeshipsForMigration(List<long> apprenticeshipIds)
        {
            var apprenticeshipModels = new List<ApprenticeshipModel>();

            var apprenticeshipIdBatches = apprenticeshipIds.Batch(2000);

            foreach (var batch in apprenticeshipIdBatches)
            {
                var apprenticeshipBatch = await _dataContext.Apprenticeship
                    .Where(a => batch.Contains(a.Id))
                    .ToListAsync();

                apprenticeshipModels.AddRange(apprenticeshipBatch);
            }

            return apprenticeshipModels;
        }

        public async Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod)
        {
            var sqlParameters = collectionPeriod.Select((item, index) => new SqlParameter($"@period{index}", item)).ToList();
            var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));

            sqlParameters.Add(new SqlParameter("@ukprn", ukprn));
            sqlParameters.Add(new SqlParameter("@academicYear", academicYear));

            await _dataContext.Database.ExecuteSqlRawAsync($"DELETE FROM dbo.Training WHERE ukprn = @ukprn AND AcademicYear = @academicYear AND IlrSubmissionWindowPeriod IN ({ sqlParamName })", sqlParameters);
        }


        public async Task StoreSubmissionsData(List<TrainingModel> trainings, CancellationToken cancellationToken)
        {
            try
            {
                await SaveTrainings(trainings, cancellationToken);
            }
            catch (Exception e)
            {
                if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;

                _logger.LogInformation("Batch contained a duplicate DataLock.  Will store each individually and discard duplicate.");

                await SaveTrainingsIndividually(trainings, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SaveTrainings(IList<TrainingModel> trainings, CancellationToken cancellationToken)
        {
            var bulkConfig = new BulkConfig { SetOutputIdentity = true, PreserveInsertOrder = true, BulkCopyTimeout = 60 };

            await _dataContext.BulkInsertAsync(trainings, bulkConfig, null, cancellationToken).ConfigureAwait(false);
            
            var priceEpisodes = trainings
                .SelectMany(training =>
                {
                    foreach (var priceEpisode in training.PriceEpisodes)
                    {
                        priceEpisode.TrainingId = training.Id;
                    }
                    return training.PriceEpisodes;
                })
                .ToList();

            await _dataContext.BulkInsertAsync(priceEpisodes, bulkConfig, null, cancellationToken).ConfigureAwait(false);

            var periods = priceEpisodes
                .SelectMany(priceEpisode =>
                {
                    foreach (var period in priceEpisode.Periods)
                    {
                        period.PriceEpisodeId = priceEpisode.Id;
                    }
                    return priceEpisode.Periods;
                })
                .ToList();

            await _dataContext.BulkInsertAsync(periods, bulkConfig, null, cancellationToken).ConfigureAwait(false);
        }

        public  async Task SaveTrainingsIndividually(List<TrainingModel> models, CancellationToken cancellationToken)
        {
            var mainContext = _retryDataContextFactory.Create();

            await using var tx = await mainContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken).ConfigureAwait(false);

            foreach (var training in models)
            {
                try
                {
                    var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                    await retryDataContext.Trainings.AddAsync(training, cancellationToken).ConfigureAwait(false);
                    await retryDataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (e.IsUniqueKeyConstraintException())
                    {
                        _logger.LogInformation($"Discarding duplicate DataLock event. Event Id: {training.EventId}, Learn ref: {training.Uln}");
                        continue;
                    }
                    throw;
                }
            }

            await tx.CommitAsync(cancellationToken);
        }
    }
}
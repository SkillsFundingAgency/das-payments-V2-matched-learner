using System;
using System.Collections.Generic;
using System.Data;
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
    public interface IMatchedLearnerRepository
    {
        Task<List<TrainingModel>> GetMatchedLearnerTrainings(long ukprn, long uln);
        Task<List<DataLockEventModel>> GetDataLockEventsForMigration(long ukprn);
        Task<List<ApprenticeshipModel>> GetApprenticeshipsForMigration(List<long> apprenticeshipIds);
        Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task SaveTrainingsIndividually(List<TrainingModel> trainings);
        Task SaveTrainings(IList<TrainingModel> trainings);
        Task SaveLatestSubmissionJob(LatestSubmissionJobModel latestSubmissionJob);
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

        public async Task<List<TrainingModel>> GetMatchedLearnerTrainings(long ukprn, long uln)
        {
            return await _dataContext.Trainings
                 .Include(t => t.PriceEpisodes)
                 .ThenInclude(p => p.Periods)
                 .Where(t => t.Ukprn == ukprn && t.Uln == uln)
                 .ToListAsync();
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
            _logger.LogInformation($"Removed Previous Submissions Training Data. Ukprn: {ukprn}, AcademicYear: {academicYear}, CollectionPeriods: {string.Join(", ", collectionPeriod)} ");

            var sqlParameters = collectionPeriod.Select((item, index) => new SqlParameter($"@period{index}", item)).ToList();
            var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));

            sqlParameters.Add(new SqlParameter("@ukprn", ukprn));
            sqlParameters.Add(new SqlParameter("@academicYear", academicYear));

            await _dataContext.Database.ExecuteSqlRawAsync($"DELETE FROM dbo.Training WHERE ukprn = @ukprn AND AcademicYear = @academicYear AND IlrSubmissionWindowPeriod IN ({ sqlParamName })", sqlParameters);
        }

        public async Task SaveTrainings(IList<TrainingModel> trainings)
        {
            _logger.LogInformation($"Saving Submissions Training Data in Bulk. TrainingCount {trainings.Count}");

            var bulkConfig = new BulkConfig { SetOutputIdentity = true, PreserveInsertOrder = true, BulkCopyTimeout = 150, UseTempDB = true };

            await _dataContext.BulkInsertAsync(trainings, bulkConfig.Clone());

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

            await _dataContext.BulkInsertAsync(priceEpisodes, bulkConfig.Clone());

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

            await _dataContext.BulkInsertAsync(periods, bulkConfig);
        }

        public async Task SaveTrainingsIndividually(List<TrainingModel> trainings)
        {
            _logger.LogInformation($"Saving Submissions Training Data Individually. TrainingCount {trainings.Count}");

            var mainContext = _retryDataContextFactory.Create();

            await using var tx = await mainContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted);

            foreach (var training in trainings)
            {
                try
                {
                    var retryDataContext = _retryDataContextFactory.Create(tx.GetDbTransaction());
                    await retryDataContext.Trainings.AddAsync(training);
                    await retryDataContext.SaveChangesAsync();
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

            await tx.CommitAsync();
        }

        public async Task SaveLatestSubmissionJob(LatestSubmissionJobModel latestSubmissionJob)
        {
            _logger.LogInformation($"Saving latestSubmission Job, DcJobId {latestSubmissionJob.DcJobId}");

            try
            {
                await RemovePreviousSubmissionJob(latestSubmissionJob);

                await _dataContext.LatestSubmissionJobs.AddAsync(latestSubmissionJob);

                await _dataContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (e.IsUniqueKeyConstraintException())
                {
                    _logger.LogInformation($"Discarding duplicate latestSubmission Job. DcJobId {latestSubmissionJob.DcJobId}");
                    return;
                }

                throw;
            }
        }

        private async Task RemovePreviousSubmissionJob(LatestSubmissionJobModel latestSubmissionJob)
        {
            var collectionPeriods = new List<byte> { latestSubmissionJob.CollectionPeriod };

            if (latestSubmissionJob.CollectionPeriod != 1)
            {
                collectionPeriods.Add((byte)(latestSubmissionJob.CollectionPeriod - 1));
            }

            var sqlParameters = collectionPeriods.Select((item, index) => new SqlParameter($"@period{index}", item)).ToList();
            var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));

            sqlParameters.Add(new SqlParameter("@ukprn", latestSubmissionJob.Ukprn));
            sqlParameters.Add(new SqlParameter("@academicYear", latestSubmissionJob.AcademicYear));

            await _dataContext.Database.ExecuteSqlRawAsync($"DELETE FROM dbo.LatestSubmissionJob WHERE ukprn = @ukprn AND AcademicYear = @academicYear AND CollectionPeriod IN ({sqlParamName})", sqlParameters);
        }
    }
}
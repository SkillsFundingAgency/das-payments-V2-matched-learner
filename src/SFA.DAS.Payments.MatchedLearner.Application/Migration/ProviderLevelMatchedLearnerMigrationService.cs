using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IProviderLevelMatchedLearnerMigrationService
    {
        Task MigrateProviderScopedData(Guid migrationRunId, long ukprn);
    }
    public class ProviderLevelMatchedLearnerMigrationService : IProviderLevelMatchedLearnerMigrationService
    {
        private readonly IProviderMigrationRepository _providerMigrationRepository;
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly IMatchedLearnerDtoMapper _matchedLearnerDtoMapper;
        private readonly ILogger<ProviderLevelMatchedLearnerMigrationService> _logger;
        private readonly int _batchSize;

        public ProviderLevelMatchedLearnerMigrationService(
            IProviderMigrationRepository providerMigrationRepository,
            IMatchedLearnerRepository matchedLearnerRepository, 
            IMatchedLearnerDtoMapper matchedLearnerDtoMapper, 
            ILogger<ProviderLevelMatchedLearnerMigrationService> logger,
            int batchSize)
        {
            _providerMigrationRepository = providerMigrationRepository;
            _matchedLearnerRepository = matchedLearnerRepository;
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper;
            _logger = logger;
            _batchSize = batchSize;
        }

        public async Task MigrateProviderScopedData(Guid migrationRunId, long ukprn)
        {
            //todo - batching
            //todo - more data in metadata table: learnerCount, completionTime, whatever we need to make batching work
            //todo - check endpoint configuration

            try
            {
                var existingAttempts = await _providerMigrationRepository.GetProviderMigrationAttempts(ukprn);

                if(existingAttempts.Any(x => x.Status == MigrationStatus.Completed))
                    return;

                var areExistingFailedAttempts = existingAttempts.Any(x => x.Status != MigrationStatus.Completed);

                var providerLevelData =  await _matchedLearnerRepository.GetDataLockEventsForMigration(ukprn);

                await _providerMigrationRepository.CreateMigrationAttempt(new MigrationRunAttemptModel
                {
                    MigrationRunId = migrationRunId,
                    Status = MigrationStatus.InProgress,
                    Ukprn = ukprn,
                    LearnerCount = providerLevelData.Select(x => x.LearnerUln).Distinct().Count()
                });

                var apprenticeshipIds = providerLevelData
                    .SelectMany(d => d.PayablePeriods)
                    .Select(a => a.ApprenticeshipId ?? 0)
                    .Union( providerLevelData
                        .SelectMany(d => d.NonPayablePeriods)
                        .SelectMany(d => d.Failures)
                        .Select(f => f.ApprenticeshipId ?? 0))
                    .Distinct()
                    .ToList();

                var apprenticeships = await _matchedLearnerRepository.GetApprenticeshipsForMigration(apprenticeshipIds);

                var trainingData = _matchedLearnerDtoMapper.MapToModel(providerLevelData, apprenticeships);

                if (areExistingFailedAttempts)
                {
                    await HandleTrainingDataIndividually(trainingData, ukprn, migrationRunId);
                }
                else if (_batchSize != 0)
                {
                    await HandleBatches(trainingData, ukprn, migrationRunId);
                }
                else
                {
                    await HandleSingleBatchAndTransaction(trainingData, ukprn, migrationRunId);
                }

                await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Completed);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error while migrating provider.");
                await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Failed);
                throw;
            }
        }

        private async Task HandleBatches(List<TrainingModel> trainingData, long ukprn, Guid migrationRunId)
        {
            while (trainingData.Any())
            {
                var batch = trainingData.Take(_batchSize);
                trainingData = trainingData.Skip(_batchSize).ToList();
                await HandleSingleBatchAndTransaction(batch.ToList(), ukprn, migrationRunId);
            }
        }

        private async Task HandleSingleBatchAndTransaction(List<TrainingModel> trainingData, long ukprn, Guid migrationRunId)
        {
            try
            {
                await _matchedLearnerRepository.BeginTransactionAsync(CancellationToken.None);
                await _matchedLearnerRepository.StoreSubmissionsData(trainingData, CancellationToken.None);
                await _matchedLearnerRepository.CommitTransactionAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Rolling back transaction for batch.");
                await _matchedLearnerRepository.RollbackTransactionAsync(CancellationToken.None);
                await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Failed);
                throw;
            }
        }

        private async Task HandleTrainingDataIndividually(List<TrainingModel> trainingData, long ukprn, Guid migrationRunId)
        {
            try
            {
                await _matchedLearnerRepository.SaveTrainingsIndividually(trainingData, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to save training data individually.");
                await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Failed);
                throw;
            }
        }
    }
}
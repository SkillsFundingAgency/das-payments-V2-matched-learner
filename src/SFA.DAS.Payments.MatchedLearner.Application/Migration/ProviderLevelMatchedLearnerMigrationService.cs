using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IProviderLevelMatchedLearnerMigrationService
    {
        Task MigrateProviderScopedData(ProviderLevelMigrationRequest request);
    }
    public class ProviderLevelMatchedLearnerMigrationService : IProviderLevelMatchedLearnerMigrationService
    {
        private readonly IProviderMigrationRepository _providerMigrationRepository;
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly IMatchedLearnerDtoMapper _matchedLearnerDtoMapper;
        private readonly ILogger<ProviderLevelMatchedLearnerMigrationService> _logger;
        private readonly int _batchSize;
        private readonly IProviderLevelMigrationRequestSendWrapper _providerLevelMigrationRequestSendWrapper;

        public ProviderLevelMatchedLearnerMigrationService(
            IProviderMigrationRepository providerMigrationRepository,
            IMatchedLearnerRepository matchedLearnerRepository,
            IMatchedLearnerDtoMapper matchedLearnerDtoMapper,
            ILogger<ProviderLevelMatchedLearnerMigrationService> logger,
            int batchSize,
            IProviderLevelMigrationRequestSendWrapper providerLevelMigrationRequestSendWrapper)
        {
            _providerMigrationRepository = providerMigrationRepository;
            _matchedLearnerRepository = matchedLearnerRepository;
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper;
            _logger = logger;
            _batchSize = batchSize;
            _providerLevelMigrationRequestSendWrapper = providerLevelMigrationRequestSendWrapper;
        }

        public async Task MigrateProviderScopedData(ProviderLevelMigrationRequest request)
        {
            var migrationRunAttempt = new MigrationRunAttemptModel
            {
                MigrationRunId = request.MigrationRunId,
                Status = MigrationStatus.InProgress,
                Ukprn = request.Ukprn,
            };

            try
            {
                List<TrainingModel> currentBatch;
                if (request.IsFirstBatch)
                {
                    _logger.LogInformation($"Migrating data for provider {request.Ukprn}. Migration run {request.MigrationRunId}.");
                    currentBatch = await GetTrainingsForProvider(request.Ukprn);
                    migrationRunAttempt.TrainingCount = currentBatch.Count;

                    await _providerMigrationRepository.CreateMigrationAttempt(migrationRunAttempt);
                }
                else
                {
                    _logger.LogInformation($"Migrating batch of data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Batch {request.BatchNumber} of {request.TotalBatches}.");

                    currentBatch = request.TrainingData.ToList();

                    migrationRunAttempt.TrainingCount = currentBatch.Count;
                    migrationRunAttempt.BatchNumber = request.BatchNumber;
                    migrationRunAttempt.TotalBatches = request.TotalBatches;
                    migrationRunAttempt.BatchUlns = string.Join(',', currentBatch.Select(x => x.Uln));

                    await _providerMigrationRepository.CreateMigrationAttempt(migrationRunAttempt);
                }

                if (await HandleSingleBatchAndTransaction(currentBatch))
                {
                    _logger.LogInformation(request.IsFirstBatch ?
                        $"Successfully completed migrating data for provider {request.Ukprn}. Migration run {request.MigrationRunId}." :
                        $"Successfully completed migrating batch of data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Batch {request.BatchNumber} of {request.TotalBatches}.");
                    await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(migrationRunAttempt, MigrationStatus.Completed);
                }
                else
                {
                    _logger.LogInformation(request.IsFirstBatch ?
                        $"Failed migrating data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Splitting into batches for reprocessing." :
                        $"Failed migrating batch of data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Batch {request.BatchNumber} of {request.TotalBatches}.");
                    if (request.IsFirstBatch && _batchSize > 0)
                    {
                        await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(migrationRunAttempt, MigrationStatus.CompletedWithErrors);
                        ConvertToBatchesAndSend(currentBatch.ToList(), request.Ukprn, request.MigrationRunId);
                    }

                    //if subsequent run requeue - todo consider splitting?
                    _logger.LogInformation("Batches not queued either because this is a subsequent run of data already batched or a non zero batch size has not been configured.");
                }

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Batch {request.BatchNumber} for provider {request.Ukprn} on migration run {request.MigrationRunId} failed.");
                try
                {
                    await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(migrationRunAttempt, MigrationStatus.Failed);
                }
                catch (Exception updateException)
                {
                    _logger.LogError(updateException, $"Error updating migration status in error scenario. Batch {request.BatchNumber} for provider {request.Ukprn} on migration run {request.MigrationRunId}.");
                }
            }
        }

        private async Task<List<TrainingModel>> GetTrainingsForProvider(long ukprn)
        {
            var providerLevelData = await _matchedLearnerRepository.GetDataLockEventsForMigration(ukprn);

            var apprenticeshipIds = providerLevelData
                .SelectMany(d => d.PayablePeriods)
                .Select(a => a.ApprenticeshipId ?? 0)
                .Union(providerLevelData
                    .SelectMany(d => d.NonPayablePeriods)
                    .SelectMany(d => d.Failures)
                    .Select(f => f.ApprenticeshipId ?? 0))
                .Distinct()
                .ToList();

            var apprenticeships = await _matchedLearnerRepository.GetApprenticeshipsForMigration(apprenticeshipIds);

            return _matchedLearnerDtoMapper.MapToModel(providerLevelData, apprenticeships);
        }

        private void ConvertToBatchesAndSend(List<TrainingModel> trainingData, long ukprn, Guid migrationRunId)
        {
            var tasks = trainingData
                .GroupBy(x => x.Uln)
                .Select((trainingItems, index) => new { trainingItems, index })
                .GroupBy(x => x.index / _batchSize)
                .Select(async g =>
                {
                    await _providerLevelMigrationRequestSendWrapper.Send(new ProviderLevelMigrationRequest
                    {
                        TrainingData = g.SelectMany(batch => batch.trainingItems).ToArray(),
                        Ukprn = ukprn,
                        BatchNumber = g.Key,
                        TotalBatches = (int)Math.Ceiling((decimal)trainingData.Count/_batchSize),
                        MigrationRunId = migrationRunId
                    }).ConfigureAwait(false);
                });
            Task.WaitAll(tasks.ToArray());
        }

        private async Task<bool> HandleSingleBatchAndTransaction(List<TrainingModel> trainingData)
        {
            try
            {
                await _matchedLearnerRepository.BeginTransactionAsync();
                await _matchedLearnerRepository.SaveTrainings(trainingData);
                await _matchedLearnerRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error saving batch/provider, rolling back transaction and saving training items individually.");
                await _matchedLearnerRepository.RollbackTransactionAsync();
                return await HandleSavingTrainingDataIndividually(trainingData);
            }
        }

        private async Task<bool> HandleSavingTrainingDataIndividually(List<TrainingModel> trainingData)
        {
            try
            {
                await _matchedLearnerRepository.SaveTrainingsIndividually(trainingData);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error saving training items individually.");
                return false;
            }
        }
    }
}
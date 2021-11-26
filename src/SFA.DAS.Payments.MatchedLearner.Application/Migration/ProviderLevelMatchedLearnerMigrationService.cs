using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
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
        private readonly IEndpointInstanceFactory _endpointInstanceFactory;
        private readonly string _providerLevelMatchedLearnerMigration;

        public ProviderLevelMatchedLearnerMigrationService(
            IProviderMigrationRepository providerMigrationRepository,
            IMatchedLearnerRepository matchedLearnerRepository, 
            IMatchedLearnerDtoMapper matchedLearnerDtoMapper, 
            ILogger<ProviderLevelMatchedLearnerMigrationService> logger,
            int batchSize,
            IEndpointInstanceFactory endpointInstanceFactory,
            string providerLevelMatchedLearnerMigration)
        {
            _providerMigrationRepository = providerMigrationRepository;
            _matchedLearnerRepository = matchedLearnerRepository;
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper;
            _logger = logger;
            _batchSize = batchSize;
            _endpointInstanceFactory = endpointInstanceFactory;
            _providerLevelMatchedLearnerMigration = providerLevelMatchedLearnerMigration;
        }

        private async Task HandleFirstRunForProvider()
        {

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

        private async Task ConvertToBatchesAndSend(List<TrainingModel> trainingData, long ukprn, Guid migrationRunId)
        {
            var endpointInstance = await _endpointInstanceFactory.GetEndpointInstance();

            var tasks = trainingData
                .GroupBy(x => x.Uln)
                .Select((trainingItems, index) => new { trainingItems, index })
                .GroupBy(x => x.index / _batchSize)
                .Select(async g =>
                {
                    var options = new SendOptions();
                    options.SetDestination(_providerLevelMatchedLearnerMigration);
                    await endpointInstance.Send(new ProviderLevelMigrationRequest
                    {
                        TrainingData = g.SelectMany(batch => batch.trainingItems).ToArray(),
                        Ukprn = ukprn,
                        BatchNumber = g.Key,
                        TotalBatches = g.Count(),
                        MigrationRunId = migrationRunId
                    }, options).ConfigureAwait(false);
                });
            Task.WaitAll(tasks.ToArray());
        }

        //private async Task SendBatches(List<ProviderLevelMigrationRequest> batches)
        //{
        //    var endpointInstance = await _endpointInstanceFactory.GetEndpointInstance();
        //    foreach (var request in batches)
        //    {
        //        var options = new SendOptions();
        //        options.SetDestination(_providerLevelMatchedLearnerMigration);
        //        await endpointInstance.Send(request, options).ConfigureAwait(false);
        //    }
        //}

        private async Task<bool> HandleSingleBatchAndTransaction(List<TrainingModel> trainingData)
        {
            try
            {
                await _matchedLearnerRepository.BeginTransactionAsync(CancellationToken.None);
                await _matchedLearnerRepository.SaveTrainings(trainingData, CancellationToken.None);
                await _matchedLearnerRepository.CommitTransactionAsync(CancellationToken.None);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error saving batch, rolling back transaction for batch.");
                await _matchedLearnerRepository.RollbackTransactionAsync(CancellationToken.None);
                try
                {
                    await _matchedLearnerRepository.SaveTrainingsIndividually(trainingData, CancellationToken.None);
                    return true;
                }
                catch (Exception exception2)
                {
                    Console.WriteLine(exception2);
                    return false;
                }
            }
        }

        public async Task MigrateProviderScopedData(ProviderLevelMigrationRequest request)
        {
            //todo - check endpoint configuration

            try
            {
                List<TrainingModel> currentBatch;
                if (request.IsFirstBatch)
                {
                    _logger.LogInformation($"Migrating data for provider {request.Ukprn}. Migration run {request.MigrationRunId}.");
                    currentBatch = await GetTrainingsForProvider(request.Ukprn);

                    await _providerMigrationRepository.CreateMigrationAttempt(new MigrationRunAttemptModel
                    {
                        MigrationRunId = request.MigrationRunId,
                        Status = MigrationStatus.InProgress,
                        Ukprn = request.Ukprn,
                        TrainingCount = currentBatch.Count
                    });
                }
                else
                {
                    _logger.LogInformation($"Migrating batch of data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Batch {request.BatchNumber} of {request.TotalBatches}.");
                    currentBatch = request.TrainingData.ToList();
                    await _providerMigrationRepository.CreateMigrationAttempt(new MigrationRunAttemptModel
                    {
                        MigrationRunId = request.MigrationRunId,
                        Status = MigrationStatus.InProgress,
                        Ukprn = request.Ukprn,
                        TrainingCount = currentBatch.Count,
                        BatchNumber = request.BatchNumber,
                        TotalBatches = request.TotalBatches,
                        BatchUlns = string.Join(',', currentBatch.Select(x => x.Uln)) //NB ulns could appear in multiple batches for a provider
                    });
                }

                if (await HandleSingleBatchAndTransaction(currentBatch))
                {
                    _logger.LogInformation(request.IsFirstBatch ?
                        $"Successfully completed migrating data for provider {request.Ukprn}. Migration run {request.MigrationRunId}." :
                        $"Successfully completed migrating batch of data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Batch {request.BatchNumber} of {request.TotalBatches}.");
                    await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(request.Ukprn, request.MigrationRunId, MigrationStatus.Completed, request.BatchNumber);
                }
                else
                {
                    _logger.LogInformation(request.IsFirstBatch ?
                        $"Failed migrating data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Splitting into batches for reprocessing." :
                        $"Failed migrating batch of data for provider {request.Ukprn}. Migration run {request.MigrationRunId}. Batch {request.BatchNumber} of {request.TotalBatches}.");
                    if (request.IsFirstBatch)
                    {
                        //todo mark completed with error
                        await ConvertToBatchesAndSend(currentBatch.ToList(), request.Ukprn, request.MigrationRunId);
                    }

                    //if subsequent run requeue - todo consider splitting?
                }

            }
            catch (Exception exception)
            {
                await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(request.Ukprn, request.MigrationRunId, MigrationStatus.Failed, request.BatchNumber);
                _logger.LogError(exception, $"Batch {request.BatchNumber} for provider {request.Ukprn} on migration run {request.MigrationRunId} failed.");
            }


            //try
            //{
            //    //HandleFirstRunForProvider(all data)
            //    //Insert batch of rows

            //    var existingAttempts = await _providerMigrationRepository.GetProviderMigrationAttempts(ukprn);

            //    if(existingAttempts.Any(x => x.Status == MigrationStatus.Completed))
            //        return;

            //    var areExistingFailedAttempts = existingAttempts.Any(x => x.Status != MigrationStatus.Completed);

            //    //var providerLevelData =  await _matchedLearnerRepository.GetDataLockEventsForMigration(ukprn);

            //    await _providerMigrationRepository.CreateMigrationAttempt(new MigrationRunAttemptModel
            //    {
            //        MigrationRunId = migrationRunId,
            //        Status = MigrationStatus.InProgress,
            //        Ukprn = ukprn,
            //        LearnerCount = providerLevelData.Select(x => x.LearnerUln).Distinct().Count()
            //    });

            //    //var apprenticeshipIds = providerLevelData
            //    //    .SelectMany(d => d.PayablePeriods)
            //    //    .Select(a => a.ApprenticeshipId ?? 0)
            //    //    .Union( providerLevelData
            //    //        .SelectMany(d => d.NonPayablePeriods)
            //    //        .SelectMany(d => d.Failures)
            //    //        .Select(f => f.ApprenticeshipId ?? 0))
            //    //    .Distinct()
            //    //    .ToList();

            //    //var apprenticeships = await _matchedLearnerRepository.GetApprenticeshipsForMigration(apprenticeshipIds);

            //    //var trainingData = _matchedLearnerDtoMapper.MapToModel(providerLevelData, apprenticeships);

            //    if (areExistingFailedAttempts)
            //    {
            //        await HandleTrainingDataIndividually(trainingData);
            //    }
            //    else if (_batchSize != 0)
            //    {
            //        await HandleBatches(trainingData);
            //    }
            //    else
            //    {
            //        await HandleSingleBatchAndTransaction(trainingData);
            //    }

            //    await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Completed);
            //}
            //catch (Exception exception)
            //{
            //    _logger.LogError(exception, $"Error while migrating provider.");
            //    await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Failed);
            //    throw;
            //}
        }

        //private async Task HandleBatches(List<TrainingModel> trainingData)
        //{
        //    while (trainingData.Any())
        //    {
        //        var batch = trainingData.Take(_batchSize);
        //        trainingData = trainingData.Skip(_batchSize).ToList();
        //        await HandleSingleBatchAndTransaction(batch.ToList());
        //    }
        //}

        //private async Task HandleTrainingDataIndividually(List<TrainingModel> trainingData)
        //{
        //    await _matchedLearnerRepository.SaveTrainingsIndividually(trainingData, CancellationToken.None);
        //}
    }
}
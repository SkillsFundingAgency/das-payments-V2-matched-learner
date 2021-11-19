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

        public ProviderLevelMatchedLearnerMigrationService(
            IProviderMigrationRepository providerMigrationRepository,
            IMatchedLearnerRepository matchedLearnerRepository, 
            IMatchedLearnerDtoMapper matchedLearnerDtoMapper, 
            ILogger<ProviderLevelMatchedLearnerMigrationService> logger)
        {
            _providerMigrationRepository = providerMigrationRepository;
            _matchedLearnerRepository = matchedLearnerRepository;
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper;
            _logger = logger;
        }

        public async Task MigrateProviderScopedData(Guid migrationRunId, long ukprn)
        {
            try
            {
                var existingAttempts = await _providerMigrationRepository.GetProviderMigrationAttempts(ukprn);

                if(existingAttempts.Any(x => x.Status == MigrationStatus.Completed))
                    return;

                var areExistingFailedAttempts = existingAttempts.Any(x => x.Status != MigrationStatus.Completed);

                await _providerMigrationRepository.CreateMigrationAttempt(new MigrationRunAttemptModel
                {
                    MigrationRunId = migrationRunId,
                    Status = MigrationStatus.InProgress,
                    Ukprn = ukprn
                });

                try
                {
                    var providerLevelData =  await _matchedLearnerRepository.GetDataLockEventsForMigration(ukprn);
                    

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
                        await _matchedLearnerRepository.SaveTrainingsIndividually(trainingData, CancellationToken.None);
                    }
                    else
                    {
                        await _matchedLearnerRepository.BeginTransactionAsync(CancellationToken.None);
                        await _matchedLearnerRepository.StoreSubmissionsData(trainingData, CancellationToken.None);
                        await _matchedLearnerRepository.CommitTransactionAsync(CancellationToken.None);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception,$"Error while attempting to migrate provider.");

                    if (!areExistingFailedAttempts)
                    {
                        _logger.LogError($"Rolling back transaction.");
                        await _matchedLearnerRepository.RollbackTransactionAsync(CancellationToken.None);
                    }

                    await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Failed);
                    
                    throw;
                }

                await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Completed);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while preparing to migrate provider.");
                throw;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public ProviderLevelMatchedLearnerMigrationService(IProviderMigrationRepository providerMigrationRepository, IMatchedLearnerRepository matchedLearnerRepository, IMatchedLearnerDtoMapper matchedLearnerDtoMapper)
        {
            _providerMigrationRepository = providerMigrationRepository;
            _matchedLearnerRepository = matchedLearnerRepository;
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper;
        }

        public async Task MigrateProviderScopedData(Guid migrationRunId, long ukprn)
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
                //todo extract the datalock data for the given provider
                var providerLevelData =  await _matchedLearnerRepository.GetDataLockEventsForMigration(ukprn);
                var apprenticeships = await _matchedLearnerRepository.GetApprenticeshipsForMigration(new List<long>()); //todo these ids need to come from data lock events for the provider

                //todo transform that data into the new schema/model set
                var trainingData = _matchedLearnerDtoMapper.MapToModel(providerLevelData, apprenticeships);


                //todo load that data into the new tables (either bulk or single insert mode)
                await _matchedLearnerRepository.StoreSubmissionsData(trainingData, CancellationToken.None, areExistingFailedAttempts);

                await _matchedLearnerRepository.CommitTransactionAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                //todo log error
                //rollback transaction
                //set the status to failed
                await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Failed);
                throw;
            }

            await _providerMigrationRepository.UpdateMigrationRunAttemptStatus(ukprn, migrationRunId, MigrationStatus.Completed);
        }
    }
}
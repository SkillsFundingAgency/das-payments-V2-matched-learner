using System;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IMigrationStatusRepository _migrationStatusRepository;

        public ProviderLevelMatchedLearnerMigrationService(IMigrationStatusRepository migrationStatusRepository)
        {
            _migrationStatusRepository = migrationStatusRepository;
        }

        public async Task MigrateProviderScopedData(Guid migrationRunId, long ukprn)
        {
            var existingAttempts = await _migrationStatusRepository.GetProviderMigrationAttempts(ukprn);

            if(existingAttempts.Any(x => x.Status == MigrationStatus.Completed))
                return;

            var singleInsertMode = existingAttempts.Any(x => x.Status != MigrationStatus.Completed);

            await _migrationStatusRepository.CreateMigrationAttempt(new MigrationRunAttemptModel
            {
                Identifier = migrationRunId,
                Status = MigrationStatus.InProgress,
                Ukprn = ukprn
            });

            try
            {
                //todo extract the datalock data for the given provider
                //todo transform that data into the new schema/model set
                //todo load that data into the new tables (either bulk or single insert mode)
            }
            catch (Exception e)
            {
                //todo log error
                //rollback transaction
                //set the status to failed
                await _migrationStatusRepository.UpdateMigrationRunAttempt(ukprn, migrationRunId, MigrationStatus.Failed);
                throw;
            }

            await _migrationStatusRepository.UpdateMigrationRunAttempt(ukprn, migrationRunId, MigrationStatus.Completed);
        }
    }
}
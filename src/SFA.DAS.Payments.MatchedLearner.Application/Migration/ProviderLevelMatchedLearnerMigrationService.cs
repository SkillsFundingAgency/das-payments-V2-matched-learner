using System;
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
        

        //first run - provider 1-5 succeeded, provider 6 failed, provider 7-10 never ran
        //second run - providers 1-5 are ignored, provider 6

        //provider 6
        //first record - migration run aaaa-, ukprn 100006, status failed
        //second record - migration run bbbb-, ukprn 100006, status inprogress

        public async Task MigrateProviderScopedData(Guid migrationRunId, long ukprn)
        {
            var existingStatusModel = await _migrationStatusRepository.GetProviderMigrationStatusModel(ukprn);

            if (existingStatusModel == null)
            {
                await _migrationStatusRepository.CreateMigrationStatusModel(new MigrationStatusModel
                {
                    Identifier = migrationRunId,
                    Status = MigrationStatus.InProgress,
                    Ukprn = ukprn
                });
            }
            else switch (existingStatusModel.Status)
            {
                case MigrationStatus.Completed:
                    //todo log already completed for this provider
                    return;
                case MigrationStatus.Failed:
                case MigrationStatus.InProgress:
                    //todo flag single insert mode
                    break;
            }

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
                await _migrationStatusRepository.UpdateStatus(ukprn, MigrationStatus.Failed);
                throw;
            }

            await _migrationStatusRepository.UpdateStatus(ukprn, MigrationStatus.Completed);
        }
    }
}
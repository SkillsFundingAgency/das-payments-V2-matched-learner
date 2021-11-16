using System;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IProviderLevelMatchedLearnerMigrationService
    {
        Task MigrateProviderScopedData(Guid migrationRunId, long ukprn);
    }
    public class ProviderLevelMatchedLearnerMigrationService : IProviderLevelMatchedLearnerMigrationService
    {
        public async Task MigrateProviderScopedData(Guid migrationRunId, long ukprn)
        {
            //todo check if migration has already succeeded, if so return
            //todo check if ukprn previously failed - single insert mode

            //todo extract the datalock data for the given provider
            //todo transform that data into the new schema/model set
            //todo load that data into the new tables (either bulk or single insert mode)
            //todo update the metadata tables appropriately
            throw new NotImplementedException();
        }
    }
}
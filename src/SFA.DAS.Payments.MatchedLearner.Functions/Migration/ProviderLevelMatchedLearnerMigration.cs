using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public class ProviderLevelMatchedLearnerMigration
    {
        private readonly IProviderLevelMatchedLearnerMigrationService _providerLevelMatchedLearnerMigrationService;

        public ProviderLevelMatchedLearnerMigration(IProviderLevelMatchedLearnerMigrationService providerLevelMatchedLearnerMigrationService)
        {
            _providerLevelMatchedLearnerMigrationService = providerLevelMatchedLearnerMigrationService;
        }

        [FunctionName("ProviderLevelMatchedLearnerMigration")]
        public async Task RunProviderLevelMatchedLearnerMigration([ServiceBusTrigger("%MigrationQueue%", Connection = "PaymentsServiceBusConnectionString")] string message)
        {
            var request = JsonConvert.DeserializeObject<ProviderLevelMigrationRequest>(message);
            await _providerLevelMatchedLearnerMigrationService.MigrateProviderScopedData(request.MigrationRunId, request.Ukprn);
        }
    }
}

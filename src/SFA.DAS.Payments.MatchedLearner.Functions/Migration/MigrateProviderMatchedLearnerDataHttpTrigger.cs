using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public class MigrateProviderMatchedLearnerDataHttpTrigger
    {
        private readonly IMigrateProviderMatchedLearnerDataTriggerService _migrateProviderMatchedLearnerDataTriggerService;

        public MigrateProviderMatchedLearnerDataHttpTrigger(IMigrateProviderMatchedLearnerDataTriggerService migrateProviderMatchedLearnerDataTriggerService)
        {
            _migrateProviderMatchedLearnerDataTriggerService = migrateProviderMatchedLearnerDataTriggerService ?? throw new ArgumentNullException(nameof(migrateProviderMatchedLearnerDataTriggerService));
        }

        [FunctionName("HttpTriggerMatchedLearnerMigration")]
        public async Task HttpTriggerMatchedLearnerMigration(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            await _migrateProviderMatchedLearnerDataTriggerService.TriggerMigration();
        }
    }
}

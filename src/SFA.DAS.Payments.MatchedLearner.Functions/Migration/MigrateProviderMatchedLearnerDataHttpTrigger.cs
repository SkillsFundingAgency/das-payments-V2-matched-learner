using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public class MigrateProviderMatchedLearnerDataHttpTrigger
    {
        private readonly IFunctionEndpoint _endpoint;

        private readonly IMigrateProviderMatchedLearnerDataTriggerService _migrateProviderMatchedLearnerDataTriggerService;

        public MigrateProviderMatchedLearnerDataHttpTrigger(IMigrateProviderMatchedLearnerDataTriggerService migrateProviderMatchedLearnerDataTriggerService, IFunctionEndpoint endpoint)
        {
            _migrateProviderMatchedLearnerDataTriggerService = migrateProviderMatchedLearnerDataTriggerService ?? throw new ArgumentNullException(nameof(migrateProviderMatchedLearnerDataTriggerService));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        [FunctionName("HttpTriggerMatchedLearnerMigration")]
        public async Task HttpTriggerMatchedLearnerMigration([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest httpRequest)
        {
            await _endpoint.Send(new StartProviderMatchedLearnerDataMigration(), SendLocally.Options, null);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public class MigrateProviderMatchedLearnerDataServiceBusTrigger
    {
        private readonly IMigrateProviderMatchedLearnerDataService _migrateProviderMatchedLearnerDataService;
        private readonly ILogger<MigrateProviderMatchedLearnerDataServiceBusTrigger> _logger;

        public MigrateProviderMatchedLearnerDataServiceBusTrigger(IMigrateProviderMatchedLearnerDataService migrateProviderMatchedLearnerDataService, ILogger<MigrateProviderMatchedLearnerDataServiceBusTrigger> logger)
        {
            _migrateProviderMatchedLearnerDataService = migrateProviderMatchedLearnerDataService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("MigrateProviderMatchedLearnerDataHandler")]
        public async Task Handle([ServiceBusTrigger("%MigrationQueue%", Connection = "PaymentsServiceBusConnectionString")] string message)
        {
            try
            {
                var migrateProviderMatchedLearnerData = JsonConvert.DeserializeObject<MigrateProviderMatchedLearnerData>(message);

                if (migrateProviderMatchedLearnerData == null) throw new InvalidOperationException("Error parsing SubmissionJobSucceeded message");

                await _migrateProviderMatchedLearnerDataService.MigrateProviderScopedData(migrateProviderMatchedLearnerData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Handling Submission Succeeded Event, Please see internal exception for more info");
                throw;
            }
        }
    }
}

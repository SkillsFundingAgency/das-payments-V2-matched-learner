using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class ImportMatchedLearnerDataServiceBusTrigger
    {
        private readonly IMatchedLearnerDataImportService _matchedLearnerDataImportService;
        private readonly ILogger<ImportMatchedLearnerDataServiceBusTrigger> _logger;

        public ImportMatchedLearnerDataServiceBusTrigger(IMatchedLearnerDataImportService matchedLearnerDataImportService, ILogger<ImportMatchedLearnerDataServiceBusTrigger> logger)
        {
            _matchedLearnerDataImportService = matchedLearnerDataImportService ?? throw new ArgumentNullException(nameof(matchedLearnerDataImportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("ImportMatchedLearnerDataHandler")]
        public async Task RunServiceBusTrigger([ServiceBusTrigger("%MatchedLearnerImportQueue%", Connection = "PaymentsServiceBusConnectionString")] string message)
        {
            try
            {
                var importMatchedLearnerData = JsonConvert.DeserializeObject<ImportMatchedLearnerData>(message);

                if (importMatchedLearnerData == null) throw new InvalidOperationException("Error parsing ImportMatchedLearnerData message");

                await _matchedLearnerDataImportService.Import(importMatchedLearnerData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error Handling ImportMatchedLearnerData, Inner Exception {e}");
                throw;
            }
        }
    }
}
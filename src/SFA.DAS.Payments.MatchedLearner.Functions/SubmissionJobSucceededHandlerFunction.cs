using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class SubmissionSucceededHandlerFunction
    {
        private readonly IMatchedLearnerDataImportService _matchedLearnerDataImportService;
        private readonly ILogger<SubmissionSucceededHandlerFunction> _logger;

        public SubmissionSucceededHandlerFunction(IMatchedLearnerDataImportService matchedLearnerDataImportService,  ILogger<SubmissionSucceededHandlerFunction> logger)
        {
            _matchedLearnerDataImportService = matchedLearnerDataImportService ?? throw new ArgumentNullException(nameof(matchedLearnerDataImportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("SubmissionSucceededHandler")]
        public async Task Run([ServiceBusTrigger("%MatchedLearnerQueue%", Connection = "PaymentsServiceBusConnectionString")] string message)
        {
            try
            {
                var submissionSucceededEvent = JsonConvert.DeserializeObject<SubmissionJobSucceeded>(message);

                if (submissionSucceededEvent == null) throw new InvalidOperationException("Error parsing SubmissionJobSucceeded message");

                await _matchedLearnerDataImportService.Import(submissionSucceededEvent);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Handling Submission Succeeded Event, Please see internal exception for more info");
            }
        }
    }
}

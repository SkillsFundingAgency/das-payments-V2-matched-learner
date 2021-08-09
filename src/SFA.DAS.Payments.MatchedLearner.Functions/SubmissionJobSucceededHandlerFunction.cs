using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.SubmissionJobs.Messages;

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
        public async Task Run([ServiceBusTrigger("%MatchedLearner:MatchedLearnerQueue%", Connection = "MatchedLearnerServiceBusConnectionString")] string message)
        {
            try
            {
                var submissionSucceededEvent = JsonConvert.DeserializeObject<SubmissionSucceededEvent>(message);

                await _matchedLearnerDataImportService.Import(submissionSucceededEvent.Ukprn, submissionSucceededEvent.CollectionPeriod, submissionSucceededEvent.AcademicYear);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Handling Submission Succeeded Event, Please see internal exception for more info", e);
            }
        }
    }
}

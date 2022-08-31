using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class SubmissionSucceededServiceBusTrigger
    {
        private readonly ISubmissionSucceededDelayedImportService _submissionSucceededDelayedImportService;
        private readonly ILogger<SubmissionSucceededServiceBusTrigger> _logger;

        public SubmissionSucceededServiceBusTrigger(ISubmissionSucceededDelayedImportService submissionSucceededDelayedImportService, ILogger<SubmissionSucceededServiceBusTrigger> logger)
        {
            _submissionSucceededDelayedImportService = submissionSucceededDelayedImportService ?? throw new ArgumentNullException(nameof(submissionSucceededDelayedImportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("SubmissionSucceededHandler")]
        public async Task RunServiceBusTrigger([ServiceBusTrigger("%MatchedLearnerQueue%", Connection = "PaymentsServiceBusConnectionString")] string message)
        {
            try
            {
                var submissionSucceededEvent = JsonConvert.DeserializeObject<SubmissionJobSucceeded>(message.Trim('\uFEFF', '\u200B'));

                if (submissionSucceededEvent == null) throw new InvalidOperationException("Error parsing SubmissionJobSucceeded message");

                await _submissionSucceededDelayedImportService.ProcessSubmissionSucceeded(submissionSucceededEvent);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error Handling Submission Succeeded Event, Inner Exception {e}");
                throw;
            }
        }
    }
}

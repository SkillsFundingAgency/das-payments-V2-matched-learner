using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class SubmissionSucceededHandler : IHandleMessages<SubmissionJobSucceeded>
    {
        private readonly ISubmissionSucceededDelayedImportService _submissionSucceededDelayedImportService;
        private readonly ILogger<SubmissionSucceededHandler> _logger;

        public SubmissionSucceededHandler(ISubmissionSucceededDelayedImportService submissionSucceededDelayedImportService, ILogger<SubmissionSucceededHandler> logger)
        {
            _submissionSucceededDelayedImportService = submissionSucceededDelayedImportService ?? throw new ArgumentNullException(nameof(submissionSucceededDelayedImportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(SubmissionJobSucceeded message, IMessageHandlerContext context)
        {
            try
            {
                await _submissionSucceededDelayedImportService.ProcessSubmissionSucceeded(message, context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error Handling Submission Succeeded Event, Inner Exception {exception}");
                throw;
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class SubmissionSucceededHandlerFunction
    {
        private readonly IMatchedLearnerDataImportService _matchedLearnerDataImportService;
        private readonly ILogger<SubmissionSucceededHandlerFunction> _logger;

        public SubmissionSucceededHandlerFunction(IMatchedLearnerDataImportService matchedLearnerDataImportService,  ILogger<SubmissionSucceededHandlerFunction> logger)
        {
            _matchedLearnerDataImportService = matchedLearnerDataImportService ?? throw new ArgumentNullException(nameof(matchedLearnerDataImportService));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("SubmissionSucceededHandler")]
        public async Task Run([ServiceBusTrigger("%MatchedLearnerQueue%", Connection = "ServiceBusConnectionString")] string myQueueItem)
        {
            await _matchedLearnerDataImportService.Import(1, 1, 1);
        }
    }
}

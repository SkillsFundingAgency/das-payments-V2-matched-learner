using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    //public class SubmissionJobSucceededHandlerFunction
    //{
    //    private readonly ILogger<SubmissionJobSucceededHandlerFunction> _logger;

    //    public SubmissionJobSucceededHandlerFunction(ILogger<SubmissionJobSucceededHandlerFunction> logger)
    //    {
    //        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    //    }

    //    [FunctionName("SubmissionJobSucceededHandler")]
    //    public void Run([ServiceBusTrigger("%MatchedLearnerQueue%", Connection = "ServiceBusConnectionString")]string myQueueItem)
    //    {
    //        _logger.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
    //    }
    //}
}

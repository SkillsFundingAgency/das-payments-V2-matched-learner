using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class ServiceBusTriggerEndPoint
    {
        private readonly IFunctionEndpoint _endpoint;
        private readonly ILogger<ServiceBusTriggerEndPoint> _logger;

        public ServiceBusTriggerEndPoint(IFunctionEndpoint endpoint, ILogger<ServiceBusTriggerEndPoint> logger)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("ServiceBusTriggerEndPoint")]
        public async Task RunServiceBusTrigger([ServiceBusTrigger("%MatchedLearnerQueue%", Connection = "PaymentsServiceBusConnectionString", AutoComplete = true)] Message message, ExecutionContext context)
        {
            try
            {
                await _endpoint.Process(message, context, null, _logger);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error Handling Submission Succeeded Event, Inner Exception {exception}");
                throw;
            }
        }
    }
}

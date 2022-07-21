using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Features;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestEndpoint
    {
        private IEndpointInstance _endpointInstance;
        private readonly TestApplicationSettings _testConfiguration;
        public TestEndpoint()
        {
            _testConfiguration = TestConfiguration.TestApplicationSettings;
        }

        public async Task<IEndpointInstance> Start()
        {
            if (_endpointInstance != null)
                return _endpointInstance;

            var endpointConfiguration = CreateEndpointConfiguration();

            _endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            return _endpointInstance;
        }

        public async Task Stop()
        {
            await _endpointInstance.Stop();
        }

        private EndpointConfiguration CreateEndpointConfiguration()
        {
            var endpointConfiguration = new EndpointConfiguration("MatchedLearner.Functions.AcceptanceTests");

            var conventions = endpointConfiguration.Conventions();

            conventions.DefiningEventsAs(t => typeof(SubmissionJobSucceeded).IsAssignableFrom(t));

            var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();

            if (string.IsNullOrWhiteSpace(_testConfiguration.MatchedLearnerStorageAccountConnectionString))
                throw new InvalidOperationException("MatchedLearnerStorageAccountConnectionString is null");

            persistence.ConnectionString(_testConfiguration.MatchedLearnerStorageAccountConnectionString);

            endpointConfiguration.DisableFeature<TimeoutManager>();

            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

            if (string.IsNullOrWhiteSpace(_testConfiguration.PaymentsServiceBusConnectionString))
                throw new InvalidOperationException("PaymentsServiceBusConnectionString is null");

            transport.ConnectionString(_testConfiguration.PaymentsServiceBusConnectionString)
                .Transactions(TransportTransactionMode.ReceiveOnly)
                .SubscriptionRuleNamingConvention(rule => rule.Name.Split('.').LastOrDefault() ?? rule.Name);

            var noBomEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
            var serialization = endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            serialization.WriterCreator(stream =>
            {
                var streamWriter = new StreamWriter(stream, noBomEncoding);
                return new JsonTextWriter(streamWriter)
                {
                    Formatting = Formatting.None
                };
            });

            endpointConfiguration.EnableInstallers();

            endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);

            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            assemblyScanner.ThrowExceptions = false;

            endpointConfiguration.DisableFeature<AutoSubscribe>();
            
            endpointConfiguration.SendOnly();

            return endpointConfiguration;
        }

        public async Task PublishSubmissionSucceededEvent(long ukprn, short academicYear, byte collectionPeriod)
        {
            await _endpointInstance.Publish(new SubmissionJobSucceeded
            {
                Ukprn = ukprn,
                CollectionPeriod = collectionPeriod,
                AcademicYear = academicYear,
                JobId = 123,
                IlrSubmissionDateTime = DateTime.Now
            });
        }
    }
}
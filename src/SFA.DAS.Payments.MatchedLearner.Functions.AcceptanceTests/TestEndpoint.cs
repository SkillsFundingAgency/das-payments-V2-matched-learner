using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.Monitoring.SubmissionJobs.Messages;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestEndpoint
    {
        private IEndpointInstance _endpointInstance;
        private readonly IApplicationSettings _testConfiguration;
        public TestEndpoint()
        {
            _testConfiguration = TestConfiguration.ApplicationSettings;
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

            conventions.DefiningEventsAs(t => typeof(SubmissionSucceededEvent).IsAssignableFrom(t));

            var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();

            persistence.ConnectionString(_testConfiguration.AzureWebJobsStorage);

            endpointConfiguration.DisableFeature<TimeoutManager>();

            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

            transport.ConnectionString(_testConfiguration.MatchedLearnerServiceBusConnectionString)
                .Transactions(TransportTransactionMode.ReceiveOnly)
                .SubscriptionRuleNamingConvention(rule => rule.Name.Split('.').LastOrDefault() ?? rule.Name);

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            endpointConfiguration.EnableInstallers();

            endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);

            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            assemblyScanner.ThrowExceptions = false;

            return endpointConfiguration;
        }

        public async Task PublishSubmissionSucceededEvent(long ukprn, short academicYear, byte collectionPeriod)
        {
            await _endpointInstance.Publish(new SubmissionSucceededEvent
            {
                Ukprn = ukprn,
                CollectionPeriod = collectionPeriod,
                AcademicYear = academicYear
            });
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
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

            _endpointInstance = await Endpoint.Start(endpointConfiguration);

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

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

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
                JobId = 123
            });
        }
        
        public async Task PublishProviderLevelMigrationRequest(long ukprn)
        {
            await _endpointInstance.Publish(new ProviderLevelMigrationRequest
            {
                Ukprn = ukprn,
                MigrationRunId = Guid.NewGuid(),
            });
        }
    }
}
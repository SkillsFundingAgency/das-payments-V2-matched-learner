using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestEndpoint
    {
        private bool _endpointStarted;
        private IEndpointInstance _paymentsEndpointInstance;
        private IEndpointInstance _matchedLearnerEndpointInstance;
        private readonly TestApplicationSettings _testConfiguration;
        public TestEndpoint()
        {
            _testConfiguration = TestConfiguration.TestApplicationSettings;
        }

        public async Task Start()
        {
            if (_endpointStarted)
                return;

            var paymentEndpointConfiguration = CreateEndpointConfiguration(_testConfiguration.PaymentsServiceBusConnectionString, "payments-SubmissionJobSucceeded");

            var matchedLearnerEndpointConfiguration = CreateEndpointConfiguration(_testConfiguration.MatchedLearnerServiceBusConnectionString, "matchedLaener-MigrateProviderMatchedLearnerData");

            _paymentsEndpointInstance = await Endpoint.Start(paymentEndpointConfiguration);
            _matchedLearnerEndpointInstance = await Endpoint.Start(matchedLearnerEndpointConfiguration);

            _endpointStarted = true;
        }

        public async Task Stop()
        {
            await _paymentsEndpointInstance.Stop();
        }

        private EndpointConfiguration CreateEndpointConfiguration(string serviceBusConnectionString, string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);

            var conventions = endpointConfiguration.Conventions();

            conventions.DefiningEventsAs(t => typeof(SubmissionJobSucceeded).IsAssignableFrom(t));
            conventions.DefiningCommandsAs(t => typeof(MigrateProviderMatchedLearnerData).IsAssignableFrom(t) || typeof(ImportMatchedLearnerData).IsAssignableFrom(t));

            var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();

            if (string.IsNullOrWhiteSpace(_testConfiguration.MatchedLearnerStorageAccountConnectionString))
                throw new InvalidOperationException("MatchedLearnerStorageAccountConnectionString is null");

            persistence.ConnectionString(_testConfiguration.MatchedLearnerStorageAccountConnectionString);

            endpointConfiguration.DisableFeature<TimeoutManager>();

            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

            if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
                throw new InvalidOperationException("ServiceBusConnectionString is null");

            transport.ConnectionString(serviceBusConnectionString)
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
            await _paymentsEndpointInstance.Publish(new SubmissionJobSucceeded
            {
                Ukprn = ukprn,
                CollectionPeriod = collectionPeriod,
                AcademicYear = academicYear,
                JobId = 123,
                IlrSubmissionDateTime = DateTime.Now
            });
        }
        
        public async Task PublishProviderLevelMigrationRequest(long ukprn)
        {
            var options = new SendOptions();
            options.SetDestination(_testConfiguration.MigrationQueue);
            await _matchedLearnerEndpointInstance.Send(new MigrateProviderMatchedLearnerData
            {
                Ukprn = ukprn,
                MigrationRunId = Guid.NewGuid(),
            }, options);
        }
    }
}
using System;
using System.Threading;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Payments.MatchedLearner.Functions;
using SFA.DAS.Payments.MatchedLearner.Functions.Ioc;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions();

            var configuration = builder.GetContext().Configuration;

            var config = configuration.InitialiseConfigure();

            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            var applicationSettings = builder.Services.AddApplicationSettings(config);

            builder.Services.AddNLog(applicationSettings, "Functions");

            builder.Services.AddAppDependencies(applicationSettings);

            var managementClient = new ServiceBusAdministrationClient(applicationSettings.PaymentsServiceBusConnectionString);

            EnsureQueueAndSubscription(managementClient, applicationSettings.MatchedLearnerQueue, typeof(SubmissionJobSucceeded));

            EnsureQueueAndSubscription(managementClient, applicationSettings.MatchedLearnerImportQueue);
        }

        private static void EnsureQueueAndSubscription(ServiceBusAdministrationClient managementClient, string queue, Type messageType = null)
        {
            try
            {
                EnsureQueue(managementClient, queue);

                if (messageType == null) return;

                EnsureSubscription(managementClient, queue, messageType);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error ensuring Ensure Queue And Subscription: {e.Message}.");
                Console.WriteLine(e);
                throw;
            }
        }

        private static void EnsureQueue(ServiceBusAdministrationClient managementClient, string queue)
        {
            if (managementClient.QueueExistsAsync(queue, CancellationToken.None).GetAwaiter().GetResult()) return;

            var queueDescription = new CreateQueueOptions(queue)
            {
                DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                DeadLetteringOnMessageExpiration = true,
                LockDuration = TimeSpan.FromMinutes(5),
                MaxDeliveryCount = 1,
                MaxSizeInMegabytes = 5120,
                //Path = queue
            };

            managementClient.CreateQueueAsync(queueDescription, CancellationToken.None).GetAwaiter().GetResult();
        }

        private static void EnsureSubscription(ServiceBusAdministrationClient managementClient, string queue, Type messageType)
        {
            const string topicPath = "bundle-1";
            var ruleDescription = new CreateRuleOptions(messageType.Name, new SqlRuleFilter($"[NServiceBus.EnclosedMessageTypes] LIKE '%{messageType.FullName}%'"));

            if (managementClient.SubscriptionExistsAsync(topicPath, queue, CancellationToken.None).GetAwaiter().GetResult())
            {
                managementClient.DeleteSubscriptionAsync(topicPath, queue, CancellationToken.None).GetAwaiter().GetResult();
            }

            var subscriptionDescription = new CreateSubscriptionOptions(topicPath, queue)
            {
                DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                DeadLetteringOnMessageExpiration = true,
                LockDuration = TimeSpan.FromMinutes(5),
                MaxDeliveryCount = 1,
                SubscriptionName = queue,
                ForwardTo = queue,
                EnableBatchedOperations = false,
            };

            managementClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}

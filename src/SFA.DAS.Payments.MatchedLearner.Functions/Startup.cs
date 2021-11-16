using System;
using System.Threading;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Functions;
using SFA.DAS.Payments.MatchedLearner.Functions.Ioc;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
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

            EnsureQueueAndSubscription(applicationSettings.PaymentsServiceBusConnectionString, applicationSettings.MatchedLearnerQueue, typeof(SubmissionJobSucceeded));
            EnsureQueueAndSubscription(applicationSettings.PaymentsServiceBusConnectionString, applicationSettings.MigrationQueue, typeof(ProviderLevelMigrationRequest));
        }

        private static void EnsureQueueAndSubscription(string connection, string queue, Type messageType)
        {
            try
            {
                const string topicPath = "bundle-1";

                var manageClient = new ManagementClient(connection);

                if (!manageClient.QueueExistsAsync(queue, CancellationToken.None).GetAwaiter().GetResult())
                {
                    var queueDescription = new QueueDescription(queue)
                    {
                        DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                        EnableDeadLetteringOnMessageExpiration = true,
                        LockDuration = TimeSpan.FromMinutes(5),
                        MaxDeliveryCount = 1,
                        MaxSizeInMB = 5120,
                        Path = queue
                    };

                    manageClient.CreateQueueAsync(queueDescription, CancellationToken.None).GetAwaiter().GetResult();
                }

                var ruleDescription = new RuleDescription(messageType.Name, new SqlFilter($"[NServiceBus.EnclosedMessageTypes] LIKE '%{messageType.FullName}%'"));

                if (manageClient.SubscriptionExistsAsync(topicPath, queue, CancellationToken.None).GetAwaiter().GetResult())
                {
                    manageClient.DeleteSubscriptionAsync(topicPath, queue, CancellationToken.None).GetAwaiter().GetResult();
                }

                var subscriptionDescription = new SubscriptionDescription(topicPath, queue)
                {
                    DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                    EnableDeadLetteringOnMessageExpiration = true,
                    LockDuration = TimeSpan.FromMinutes(5),
                    MaxDeliveryCount = 1,
                    SubscriptionName = queue,
                    ForwardTo = queue,
                    EnableBatchedOperations = false,
                };

                manageClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error ensuring Ensure Queue And Subscription: {e.Message}.");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}

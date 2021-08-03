using System;
using System.Threading;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Payments.MatchedLearner.Functions;
using SFA.DAS.Payments.MatchedLearner.Functions.Ioc;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;
using SFA.DAS.Payments.Monitoring.SubmissionJobs.Messages;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var config = configuration.InitialiseConfigure();

            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddApiConfigurationSections(config);

            builder.Services.AddNLog(config);

            builder.Services.AddOptions();

            builder.Services.AddAppDependencies();

            var applicationSettings = builder.Services.GetApplicationSettings();

            EnsureQueueAndSubscription(applicationSettings,typeof(SubmissionSucceededEvent));
        }

        private static void EnsureQueueAndSubscription(IApplicationSettings settings, Type messageType)
        {
            try
            {
                var manageClient = new ManagementClient(settings.MatchedLearnerServiceBusConnectionString);

                if (!manageClient.QueueExistsAsync(settings.MatchedLearnerQueue, CancellationToken.None).GetAwaiter().GetResult())
                {
                    var queueDescription = new QueueDescription(settings.MatchedLearnerQueue)
                    {
                        DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                        EnableDeadLetteringOnMessageExpiration = true,
                        LockDuration = TimeSpan.FromMinutes(5),
                        MaxDeliveryCount = 1,
                        MaxSizeInMB = 5120,
                        Path = settings.MatchedLearnerQueue
                    };

                    manageClient.CreateQueueAsync(queueDescription, CancellationToken.None).GetAwaiter().GetResult();
                }

                if (!manageClient.SubscriptionExistsAsync("bundle-1", settings.MatchedLearnerQueue, CancellationToken.None).GetAwaiter().GetResult())
                {
                    var subscriptionDescription = new SubscriptionDescription("bundle-1", settings.MatchedLearnerQueue)
                    {
                        DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                        EnableDeadLetteringOnMessageExpiration = true,
                        LockDuration = TimeSpan.FromMinutes(5),
                        MaxDeliveryCount = 1,
                        SubscriptionName = settings.MatchedLearnerQueue,
                        ForwardTo = settings.MatchedLearnerQueue,
                        EnableBatchedOperations = false,
                    };

                    var ruleDescription = new RuleDescription(messageType.Name, new SqlFilter($"[NServiceBus.EnclosedMessageTypes] LIKE '%{messageType.FullName}%'"));

                    manageClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription, CancellationToken.None).GetAwaiter().GetResult();
                }
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

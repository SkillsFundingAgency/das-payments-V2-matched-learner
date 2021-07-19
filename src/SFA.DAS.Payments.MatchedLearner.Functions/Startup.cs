using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Functions;
using SFA.DAS.Payments.MatchedLearner.Functions.Ioc;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

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

            builder.UseNServiceBus(() =>
            {
                var applicationSettings = builder.Services.GetApplicationSettings();
                
                Environment.SetEnvironmentVariable("AzureWebJobsServiceBus", applicationSettings.MatchedLearnerServiceBusConnectionString );

                var endpointConfiguration = new ServiceBusTriggeredEndpointConfiguration(applicationSettings.ServiceName);

                var assemblyScanner = endpointConfiguration.AdvancedConfiguration.AssemblyScanner();
                assemblyScanner.ThrowExceptions = false;

                return endpointConfiguration;
            });
        }
    }
}

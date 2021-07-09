using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

[assembly: FunctionsStartup(typeof(SFA.DAS.Payments.MatchedLearner.Functions.Startup))]
namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var isDevelopmentEnvironment = IsDevelopmentEnvironment(configuration);

            var config = configuration.InitialiseConfigure(isDevelopmentEnvironment);
            
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddApiConfigurationSections(config);

            builder.Services.AddOptions();

            builder.Services.AddAppDependencies();

            builder.Services.AddNLog(isDevelopmentEnvironment);

            builder.UseNServiceBus(() =>
            {
                var applicationSettings = builder.Services.GetApplicationSettings();

                var serviceBusTriggeredEndpointConfiguration = new ServiceBusTriggeredEndpointConfiguration(applicationSettings.EndpointName);

                var endpointConfiguration = serviceBusTriggeredEndpointConfiguration.AdvancedConfiguration;

                endpointConfiguration.UniquelyIdentifyRunningInstance()
                    .UsingCustomDisplayName(applicationSettings.EndpointName)
                    .UsingCustomIdentifier(CreateCustomIdentifier(applicationSettings.EndpointName));

                // Look for license as an environment variable
                var licenseText = applicationSettings.DasNServiceBusLicenseKey;
                if (!string.IsNullOrWhiteSpace(licenseText))
                {
                    endpointConfiguration.License(licenseText);
                }

                serviceBusTriggeredEndpointConfiguration.Transport.ConnectionString(applicationSettings.ServiceBusConnectionString);

                return serviceBusTriggeredEndpointConfiguration;
            });
        }

        private static bool IsDevelopmentEnvironment(IConfiguration configuration)
        {
            return Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID").Equals("Development", StringComparison.CurrentCultureIgnoreCase);
        }

        public static Guid CreateCustomIdentifier(string data)
        {
            // use MD5 hash to get a 16-byte hash of the string
            using var provider = new MD5CryptoServiceProvider();
            var inputBytes = Encoding.Default.GetBytes(data);
            var hashBytes = provider.ComputeHash(inputBytes);
            // generate a guid from the hash:
            return new Guid(hashBytes);
        }
    }
}

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection, ApplicationSettings applicationSettings, string serviceNamePostFix)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging(options =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by default
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog($"sfa-das-payments-matchedlearner-{serviceNamePostFix}", applicationSettings.IsDevelopment);
            });

            return serviceCollection;
        }

        public static ApplicationSettings AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationSettings = configuration
                .GetSection(ApplicationSettingsKeys.MatchedLearnerConfigKey)
                .Get<ApplicationSettings>();

            if (applicationSettings == null)
                throw new InvalidOperationException("invalid Configuration, unable find 'MatchedLearner' Configuration section");

            applicationSettings.IsDevelopment = configuration.IsDevelopment();

            services.AddSingleton(applicationSettings);

            return applicationSettings;
        }

        public static IConfiguration InitialiseConfigure(this IConfiguration configuration)
        {
            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            var isDevelopmentEnvironment = configuration.IsDevelopment();

            if (!isDevelopmentEnvironment)
            {
                config.AddAzureTableStorage(options =>
                {
                    options.PreFixConfigurationKeys = false;
                    options.ConfigurationKeys = new[] { ApplicationSettingsKeys.MatchedLearnerApiKey };
                });
            }
#if DEBUG
            if (isDevelopmentEnvironment)
            {
                config.AddJsonFile("local.settings.json", optional: false);
            }
#endif
            return config.Build();
        }

        public static bool IsDevelopment(this IConfiguration configuration)
        {
            var environmentName = configuration["EnvironmentName"];

            if (!string.IsNullOrEmpty(environmentName))
                return environmentName.Equals("Development", StringComparison.CurrentCultureIgnoreCase);

            throw new ApplicationException("Configuration is not initialized correctly");
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NServiceBus;
using NServiceBus.Features;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.SqlAzureIdentityAuthentication;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPaymentsDataContext(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddTransient(provider =>
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(applicationSettings.PaymentsConnectionString)
                    .Options;

                return new PaymentsDataContext(options);
            });
        }

        public static void AddMatchedLearnerDataContext(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddMemoryCache();

            services.AddSingleton(new AzureServiceTokenProvider());

            services.AddSingleton<ISqlAzureIdentityTokenProvider, SqlAzureIdentityTokenProvider>();

            services.Decorate<ISqlAzureIdentityTokenProvider, SqlAzureIdentityTokenProviderCache>();

            services.AddSingleton<SqlAzureIdentityAuthenticationDbConnectionInterceptor>();

            services.AddTransient<IMatchedLearnerDataContextFactory>(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(new SqlConnection(applicationSettings.MatchedLearnerConnectionString))
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContextFactory(matchedLearnerOptions);
            });

            services.AddTransient(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(new SqlConnection(applicationSettings.MatchedLearnerConnectionString))
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContext(matchedLearnerOptions);
            });
        }

        public static void AddEndpointInstanceFactory(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddTransient<IEndpointInstanceFactory>(provider =>
            {
                var logger = provider.GetService<ILogger<EndpointInstanceFactory>>();

                var endpointConfiguration = new EndpointConfiguration($"{applicationSettings.MigrationQueue}");
            
                endpointConfiguration.UseTransport<AzureServiceBusTransport>().ConnectionString(applicationSettings.PaymentsServiceBusConnectionString);
                endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
                endpointConfiguration.SendOnly();

                endpointConfiguration.CustomDiagnosticsWriter(diagnostics =>
                {
                    logger.LogDebug(diagnostics);
                    return Task.CompletedTask;
                });

                endpointConfiguration.DisableFeature<TimeoutManager>();
                endpointConfiguration.EnableInstallers();

                if (!string.IsNullOrEmpty(applicationSettings.NServiceBusLicense))
                {
                    var license = WebUtility.HtmlDecode(applicationSettings.NServiceBusLicense);
                    endpointConfiguration.License(license);
                }

                return new EndpointInstanceFactory(endpointConfiguration);
            });
        }

        public static void AddNLog(this IServiceCollection serviceCollection, ApplicationSettings applicationSettings, string serviceNamePostFix)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging(options =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Debug); // this is because all logging is filtered out by default
                options.AddFilter("Microsoft.AspNetCore.Routing.Matching.DfaMatcher", LogLevel.Warning);
                options.AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Warning);
                options.AddFilter("Microsoft.AspNetCore.Routing.EndpointRoutingMiddleware", LogLevel.Warning);
                options.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
                options.AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor", LogLevel.Warning);
                options.SetMinimumLevel(LogLevel.Trace);

                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog($"sfa-das-payments-matchedlearner-{serviceNamePostFix}", applicationSettings);
            });
        }

        public static ApplicationSettings AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationSettings = configuration
                .GetSection(ApplicationSettingsKeys.MatchedLearnerConfigKey)
                .Get<ApplicationSettings>();

            applicationSettings.UseV1Api = configuration.GetValue<bool>("UseV1Api");
            applicationSettings.AppInsightsInstrumentationKey = configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

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
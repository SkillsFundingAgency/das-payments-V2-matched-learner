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
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
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
                    .UseSqlServer(applicationSettings.PaymentsConnectionString, optionsBuilder => optionsBuilder.CommandTimeout(540))
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

            services.AddSingleton(provider => new SqlAzureIdentityAuthenticationDbConnectionInterceptor(provider.GetService<ISqlAzureIdentityTokenProvider>(), applicationSettings.ConnectionNeedsAccessToken));

            services.AddTransient<IMatchedLearnerDataContextFactory>(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(new SqlConnection(applicationSettings.MatchedLearnerConnectionString), optionsBuilder => optionsBuilder.CommandTimeout(540))
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContextFactory(matchedLearnerOptions);
            });

            services.AddTransient(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(new SqlConnection(applicationSettings.MatchedLearnerConnectionString), optionsBuilder => optionsBuilder.CommandTimeout(540))
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContext(matchedLearnerOptions);
            });
        }

        public static void AddEndpointInstance(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            var endpointConfiguration = new EndpointConfiguration(applicationSettings.MatchedLearnerImportQueue);
            var conventions = endpointConfiguration.Conventions();

            conventions.DefiningCommandsAs(t => typeof(ImportMatchedLearnerData).IsAssignableFrom(t));

            endpointConfiguration.UseTransport<AzureServiceBusTransport>().ConnectionString(applicationSettings.MatchedLearnerServiceBusConnectionString);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendOnly();

            endpointConfiguration.CustomDiagnosticsWriter(diagnostics => Task.CompletedTask);

            endpointConfiguration.DisableFeature<TimeoutManager>();
            endpointConfiguration.EnableInstallers();

            if (!string.IsNullOrEmpty(applicationSettings.NServiceBusLicense))
            {
                var license = WebUtility.HtmlDecode(applicationSettings.NServiceBusLicense);
                endpointConfiguration.License(license);
            }
#if DEBUG
            //NOTE: This is required to run the function from Acceptance test project
            var assemblyScanner = endpointConfiguration.AssemblyScanner();
            assemblyScanner.ThrowExceptions = false;
#endif
            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            services.AddSingleton(endpointInstance);
        }

        public static void AddNLog(this IServiceCollection serviceCollection, ApplicationSettings applicationSettings, string serviceNamePostFix)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging(options =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Debug); // this is because all logging is filtered out by default
                options.AddFilter("Microsoft.AspNetCore.*", LogLevel.Warning);
                options.AddFilter("Host.*", LogLevel.Warning);
                options.AddFilter("Microsoft.Azure.WebJobs.Hosting.*", LogLevel.Warning);
                options.SetMinimumLevel(LogLevel.Trace);

                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog($"sfa-das-payments-matchedlearner-{serviceNamePostFix}", applicationSettings.IsDevelopment);
            });
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
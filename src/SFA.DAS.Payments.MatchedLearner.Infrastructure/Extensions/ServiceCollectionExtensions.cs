﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using NServiceBus;
using NServiceBus.Features;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.SqlAzureIdentityAuthentication;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPaymentsDataContext(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddTransient(_ =>
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(applicationSettings.PaymentsConnectionString, optionsBuilder => optionsBuilder.CommandTimeout(7200)) //7200=2hours
                    .Options;

                return new PaymentsDataContext(options);
            });
        }

        public static void AddMatchedLearnerDataContext(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddSingleton(new AzureServiceTokenProvider());

            services.AddSingleton<ISqlAzureIdentityTokenProvider, SqlAzureIdentityTokenProvider>();

            services.AddSingleton(provider => new SqlAzureIdentityAuthenticationDbConnectionInterceptor(provider.GetService<ILogger<SqlAzureIdentityAuthenticationDbConnectionInterceptor>>(), provider.GetService<ISqlAzureIdentityTokenProvider>(), applicationSettings.ConnectionNeedsAccessToken));

            services.AddTransient<IMatchedLearnerDataContextFactory>(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(new SqlConnection(applicationSettings.MatchedLearnerConnectionString), optionsBuilder => optionsBuilder.CommandTimeout(7200)) //7200=2hours
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContextFactory(matchedLearnerOptions);
            });

            services.AddTransient(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(new SqlConnection(applicationSettings.MatchedLearnerConnectionString), optionsBuilder => optionsBuilder.CommandTimeout(7200)) //7200=2hours
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContext(matchedLearnerOptions);
            });
        }

        public static void AddEndpointInstance(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            var endpointConfiguration = new EndpointConfiguration(applicationSettings.MatchedLearnerImportQueue);
            var conventions = endpointConfiguration.Conventions();

            conventions.DefiningEventsAs(t => typeof(SubmissionJobSucceeded).IsAssignableFrom(t));
            conventions.DefiningCommandsAs(t => typeof(ImportMatchedLearnerData).IsAssignableFrom(t));

            endpointConfiguration.UseTransport<AzureServiceBusTransport>().ConnectionString(applicationSettings.PaymentsServiceBusConnectionString);

            var noBomEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
            var serialization = endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            serialization.WriterCreator(stream =>
            {
                var streamWriter = new StreamWriter(stream, noBomEncoding);
                return new JsonTextWriter(streamWriter)
                {
                    Formatting = Formatting.None
                };
            });

            endpointConfiguration.SendOnly();

            endpointConfiguration.CustomDiagnosticsWriter(_ => Task.CompletedTask);

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

            serviceCollection.AddLogging(options => //NOSONAR
            {
                options.AddFilter("SFA.DAS", LogLevel.Debug); //NOSONAR this is because all logging is filtered out by default
                options.AddFilter("Microsoft.AspNetCore.*", LogLevel.Warning); //NOSONAR
                options.AddFilter("Host.*", LogLevel.Warning); //NOSONAR
                options.AddFilter("Microsoft.Azure.WebJobs.Hosting.*", LogLevel.Warning); //NOSONAR
                options.SetMinimumLevel(LogLevel.Trace); //NOSONAR

                options.AddNLog(new NLogProviderOptions //NOSONAR
                {
                    CaptureMessageTemplates = true, //NOSONAR
                    CaptureMessageProperties = true //NOSONAR
                }); //NOSONAR
                options.AddConsole(); //NOSONAR

                nLogConfiguration.ConfigureNLog($"sfa-das-payments-matchedlearner-{serviceNamePostFix}", applicationSettings.IsDevelopment); //NOSONAR
            });//NOSONAR
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
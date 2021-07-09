﻿using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Data;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Application.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationSettings GetApplicationSettings(this IServiceCollection services, IServiceProvider serviceProvider = null)
        {
            var provider = serviceProvider ?? services.BuildServiceProvider();

            var applicationSettings = provider.GetService<IApplicationSettings>();
            if (applicationSettings == null)
            {
                throw new InvalidOperationException($"invalid Configuration, unable create instance of {ApplicationSettingsKeys.MatchedLearnerConfigKey}");
            }

            return applicationSettings;
        }

        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging(options =>
            {
                var applicationSettings = options.Services.GetApplicationSettings();

                options.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by defualt
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();
                
                var isDevelopmentEnvironment = IsDevelopmentEnvironment(configuration);

                nLogConfiguration.ConfigureNLog(applicationSettings.ServiceName, isDevelopmentEnvironment);
            });

            return serviceCollection;
        }

        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            var matchedLearnerConfig = configuration
                .GetSection(ApplicationSettingsKeys.MatchedLearnerConfigKey)
                .Get<ApplicationSettings>();

            if (matchedLearnerConfig == null)
                throw new InvalidOperationException("invalid Configuration, unable find 'MatchedLearner' Configuration section");

            services.AddSingleton<IApplicationSettings>(matchedLearnerConfig);

            return services;
        }

        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddTransient<IPaymentsContext, PaymentsContext>(provider =>
            {
                var applicationSettings = GetApplicationSettings(null, provider);

                var builder = new DbContextOptionsBuilder();
                builder.UseSqlServer(applicationSettings.PaymentsConnectionString);
                return new PaymentsContext(builder.Options);
            });
            services.AddTransient<IPaymentsDataLockRepository, PaymentsDataLockRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();
            services.AddTransient<IMatchedLearnerService, MatchedLearnerService>();

            return services;
        }

        public static IConfiguration InitialiseConfigure(this IConfiguration configuration)
        {
            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();
            
            var isDevelopmentEnvironment = IsDevelopmentEnvironment(configuration);

            if (!isDevelopmentEnvironment)
            {
                config.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                });
            }
#if DEBUG
            if (isDevelopmentEnvironment)
            {
                config.AddJsonFile("local.settings.json", optional: true);
            }
#endif
            return config.Build();
        }

        public static bool IsDevelopmentEnvironment(IConfiguration configuration)
        {
            var environmentName = configuration["EnvironmentName"];
            if (string.IsNullOrEmpty(environmentName))
            {
                configuration.InitialiseConfigure();
            }

            return configuration.Equals("Development", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
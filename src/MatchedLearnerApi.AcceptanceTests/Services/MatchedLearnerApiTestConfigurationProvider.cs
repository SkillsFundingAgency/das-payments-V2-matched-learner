﻿using System;
using System.Threading;
using MatchedLearnerApi.Extensions;
using MatchedLearnerApi.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MatchedLearnerApi.AcceptanceTests.Services
{
    public static class MatchedLearnerApiTestConfigurationProvider
    {
        public static IMatchedLearnerApiConfiguration Configuration => _configuration.Value;
        
        private static Lazy<IMatchedLearnerApiConfiguration> _configuration = new Lazy<IMatchedLearnerApiConfiguration>(() =>
            {
                var services = new ServiceCollection() as IServiceCollection;
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddEnvironmentVariables();
                configurationBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
                configurationBuilder.AddJsonFile("appsettings.json");
                configurationBuilder.AddJsonFile("appsettings.Development.json", true);
                
                var configuration = configurationBuilder.Build();
                
                services.AddApiConfigurationSections(configuration);
                var provider = services.BuildServiceProvider();
                return provider.GetService<IMatchedLearnerApiConfiguration>();
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
}

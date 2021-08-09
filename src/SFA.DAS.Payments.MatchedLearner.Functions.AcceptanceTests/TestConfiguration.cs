using System.IO;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public static class TestConfiguration
    {
        static TestConfiguration()
        {
            var config = GetAzureConfiguration();

            ApplicationSettings = config
                .GetSection("MatchedLearner")
                .Get<ApplicationSettings>();

            if (!string.IsNullOrWhiteSpace(ApplicationSettings.TargetUrl)) return;

            config = GetLocalFileConfiguration();
                
            ApplicationSettings = config
                .GetSection("MatchedLearner")
                .Get<ApplicationSettings>();
        }

        public static IConfigurationRoot GetAzureConfiguration()
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddAzureTableStorage(options =>
                 {
                     options.PreFixConfigurationKeys = false;
                     options.ConfigurationKeys = new[] { ApplicationSettingsKeys.MatchedLearnerApiKey };
                 })
                 .Build();

            return config;
        }
        
        public static IConfigurationRoot GetLocalFileConfiguration()
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("local.settings.json", optional: false)
                 .Build();

            return config;
        }
        
        public static IApplicationSettings ApplicationSettings { get; }
    }
}

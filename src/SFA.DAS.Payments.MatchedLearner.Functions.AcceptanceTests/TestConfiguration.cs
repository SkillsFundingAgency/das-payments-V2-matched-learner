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
            var config = GetConfigurationRoot();

            ApplicationSettings = config
                .GetSection("MatchedLearner")
                .Get<ApplicationSettings>();
        }

        public static IConfigurationRoot GetConfigurationRoot()
        {
            return new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddAzureTableStorage(options =>
                 {
                     options.PreFixConfigurationKeys = false;
                     options.ConfigurationKeys = new[] { ApplicationSettingsKeys.MatchedLearnerApiKey };
                 })
                 .Build();
        }
        
        public static IApplicationSettings ApplicationSettings { get; }
    }
}

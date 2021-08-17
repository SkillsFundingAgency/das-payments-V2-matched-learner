using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure
{
    public static class TestConfiguration
    {
        static TestConfiguration()
        {
            GetAzureConfiguration();

            if (!string.IsNullOrWhiteSpace(TestApplicationSettings.TargetUrl)) return;

            GetLocalFileConfiguration();
        }

        public static void GetAzureConfiguration()
        {
            IConfigurationRoot config;
            try
            {

                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddAzureTableStorage(options =>
                    {
                        options.PreFixConfigurationKeys = false;
                        options.ConfigurationKeys = new[] { ApplicationSettingsKeys.MatchedLearnerApiKey };
                        options.StorageConnectionStringEnvironmentVariableName = "ConfigurationStorageConnectionStringNew";
                        options.EnvironmentNameEnvironmentVariableName = "EnvironmentNameNew";
                    })
                    .Build();
            }
            catch (StorageException)
            {
                //suppressing this as we might be running in local development environment
                return;
            }

            TestApplicationSettings = config
                .GetSection("MatchedLearner")
                .Get<TestApplicationSettings>();
        }
        
        public static void GetLocalFileConfiguration()
        {
            //NOTE: this will throw an exception if the local.settings.json is not found
            //this is a fail safe i.e. if azure storage is not fund and ths local setting is also not fund then tests can't start
            //at release stage the azure config will always be there and at development stage either azure storage or file will be mandatory
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: false)
                .Build();

            TestApplicationSettings = config
                .GetSection("MatchedLearner")
                .Get<TestApplicationSettings>();
        }

        public static TestApplicationSettings TestApplicationSettings { get; private set; } = new TestApplicationSettings();
    }
}
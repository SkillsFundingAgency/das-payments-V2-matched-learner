using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure
{
    public class TestConfiguration
    {
        static TestConfiguration()
        {
            GetReleaseFileConfiguration();

            //if (string.IsNullOrWhiteSpace(TestAzureAdClientSettings.ApiBaseUrl))
            //{
            //    GetLocalFileConfiguration();
            //}
        }

        public static void GetLocalFileConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true)
                .Build();

            TestApplicationSettings = config
                .GetSection("MatchedLearner")
                .Get<TestApplicationSettings>();
            
            TestApplicationSettings.IsDevelopment = true;
        }

        public static void GetReleaseFileConfiguration()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "release.settings.json");
            var exists = File.Exists(path);
            if (!exists)
            {
                throw new InvalidOperationException($"relase.settings.json not found at {path}");
            }

            IConfigurationRoot config;
            try
            {
                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("release.settings.json", optional: false)
                    .Build();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"unable to read or find release.settings.json from {Directory.GetCurrentDirectory()}", e);
            }

            var isTemplateValue = config.GetValue<string>("MatchedLearner:TimeToWait");
            if (string.Equals(isTemplateValue, string.Empty, StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException("release.settings.json is empty");

            if (string.Equals(isTemplateValue, "__TimeToWait__", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException($"release.settings.json token's have not been replaced with real values, file path is {path}");

            TestApplicationSettings = config
                .GetSection("MatchedLearner")
                .Get<TestApplicationSettings>();

            TestAzureAdClientSettings = config
                .GetSection("AzureAd")
                .Get<TestAzureAdClientSettings>();
        }

        public static TestApplicationSettings TestApplicationSettings { get; set; } = new TestApplicationSettings();
        public static TestAzureAdClientSettings TestAzureAdClientSettings { get; set; } = new TestAzureAdClientSettings();
    }
}
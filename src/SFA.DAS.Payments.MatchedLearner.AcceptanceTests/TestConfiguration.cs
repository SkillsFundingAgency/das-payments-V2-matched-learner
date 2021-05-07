using System.IO;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Api.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests
{
    public static class TestConfiguration
    {
        static TestConfiguration()
        {
            var config = GetConfigurationRoot();

            MatchedLearnerApiConfiguration = config
                .GetSection("MatchedLearner")
                .Get<MatchedLearnerApiConfiguration>();
        }

        public static IConfigurationRoot GetConfigurationRoot()
        {
            return new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddAzureTableStorage(options =>
                 {
                     options.PreFixConfigurationKeys = false;
                     options.ConfigurationKeys = new[] { MatchedLearnerApiConfigurationKeys.MatchedLearnerApiKey };
                 })
                 .Build();
        }
        
        public static IMatchedLearnerApiConfiguration MatchedLearnerApiConfiguration { get; }
    }
}

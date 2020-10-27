using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests
{
    public static class TestConfiguration
    {
        static TestConfiguration()
        {
            var config = GetConfigurationRoot();

            var configSection = config.GetSection("MatchedLearner");

            DasPaymentsDatabaseConnectionString = configSection["DasPaymentsDatabaseConnectionString"];
            TargetUrl = configSection["TargetUrl"];
        }

        public static IConfigurationRoot GetConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static string DasPaymentsDatabaseConnectionString { get; }
        public static string TargetUrl { get; }
    }
}

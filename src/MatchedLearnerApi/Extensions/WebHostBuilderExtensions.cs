using MatchedLearnerApi.Configuration;
using Microsoft.AspNetCore.Hosting;
using SFA.DAS.Configuration.AzureTableStorage;

namespace MatchedLearnerApi.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureDasAppConfiguration(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration(c =>
                c.AddAzureTableStorage(MatchedLearnerApiConfigurationKeys.MatchedLearnerApi));
        }
    }
}

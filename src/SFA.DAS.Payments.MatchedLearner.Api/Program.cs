using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddAzureTableStorage(options =>
                    {
                        options.PreFixConfigurationKeys = false;
                        options.ConfigurationKeys = new[] { ApplicationSettingsKeys.MatchedLearnerApiKey };
                    });

                    //NOTE: bellow option uses PreFixConfigurationKeys = true which means all the configurations are prefixed by "MatchedLearner:<key>"
                    //config.AddAzureTableStorage(ApplicationSettingsKeys.MatchedLearnerApiKey);
                })
                .UseStartup<Startup>()
                .UseNLog()
                ;
    }
}
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Payments.MatchedLearner.Api.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting up host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                global::NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddAzureTableStorage(options =>
                    {
                        options.PreFixConfigurationKeys = false;
                        options.ConfigurationKeys = new[] {MatchedLearnerApiConfigurationKeys.MatchedLearnerApiKey};
                    });

                    //NOTE: bellow option uses PreFixConfigurationKeys = true which means all the configurations are prefixed by "MatchedLearner:<key>"
                    //config.AddAzureTableStorage(MatchedLearnerApiConfigurationKeys.MatchedLearnerApiKey);
                })
                .UseStartup<Startup>()
                .UseNLog();
    }
}
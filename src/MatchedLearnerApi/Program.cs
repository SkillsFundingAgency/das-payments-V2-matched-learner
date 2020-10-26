using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NLog.Web;
using System;
using System.IO;
using MatchedLearnerApi.Configuration;
using MatchedLearnerApi.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;

namespace MatchedLearnerApi
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
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    //var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                    //config.SetBasePath(Directory.GetCurrentDirectory());
                    //config.AddJsonFile("appSettings.json", optional: false, reloadOnChange: false);
                    //config.AddJsonFile($"appSettings.{environmentName}.json", optional: true, reloadOnChange: false);
                    //config.AddEnvironmentVariables();

                    //if (!EnvironmentExtensions.IsDevelopment())
                    {
                        config.AddAzureTableStorage(options =>
                        {
                            options.PreFixConfigurationKeys = false;
                            options.ConfigurationKeys = new[] { MatchedLearnerApiConfigurationKeys.MatchedLearnerApiKey };
                        });
                        
                        //NOTE: This option uses PreFixConfigurationKeys = true which means all the configurations are prefixed by "MatchedLearner:<key>"
                        //config.AddAzureTableStorage(MatchedLearnerApiConfigurationKeys.MatchedLearnerApiKey);
                    }
                })
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .UseNLog();
    }
}
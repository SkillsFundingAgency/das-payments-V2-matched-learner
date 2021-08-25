using System.IO;
using Microsoft.ApplicationInsights.NLogTarget;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using SFA.DAS.NLog.Targets.Redis.DotNetCore;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration
{
    public class NLogConfiguration
    {
        public void ConfigureNLog(string serviceName, bool isDevelopmentEnvironment)
        {
            var config = new LoggingConfiguration();

            if (isDevelopmentEnvironment)
            {
                AddLocalTarget(config, serviceName);
            }
            else
            {
                AddRedisTarget(config, serviceName);
                AddAppInsights(config);
            }

            LogManager.Configuration = config;
        }

        private static void AddLocalTarget(LoggingConfiguration config, string serviceName)
        {
            InternalLogger.LogFile = Path.Combine(Directory.GetCurrentDirectory(), $"logs\\nlog-internal.{serviceName}.log");
            var fileTarget = new FileTarget("Disk")
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), $"logs\\{serviceName}.${{shortdate}}.log"),
                Layout = "${longdate} [${uppercase:${level}}] [${logger}] - ${message} ${onexception:${exception:format=tostring}}"
            };
            config.AddTarget(fileTarget);

            config.AddRule(GetMinLogLevel(), LogLevel.Fatal, "Disk");
        }

        private static void AddRedisTarget(LoggingConfiguration config, string appName)
        {
            var target = new RedisTarget
            {
                Name = "RedisLog",
                AppName = appName,
                EnvironmentKeyName = "EnvironmentName",
                ConnectionStringName = "LoggingRedisConnectionString",
                IncludeAllProperties = true,
                Layout = "${message}"
            };

            config.AddTarget(target);
            config.AddRule(GetMinLogLevel(), LogLevel.Fatal, "RedisLog");
        }

        private static void AddAppInsights(LoggingConfiguration config)
        {
            var target = new ApplicationInsightsTarget
            {
                Name = "AppInsightsLog"
            };

            config.AddTarget(target);
            config.AddRule(GetMinLogLevel(), LogLevel.Fatal, "AppInsightsLog");
        }

        private static LogLevel GetMinLogLevel() => LogLevel.FromString("Debug");
    }
}

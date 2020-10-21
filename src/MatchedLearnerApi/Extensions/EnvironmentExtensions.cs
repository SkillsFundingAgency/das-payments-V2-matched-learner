using System;
using Microsoft.Extensions.Hosting;

namespace MatchedLearnerApi.Extensions
{
    public class EnvironmentExtensions
    {
        public static bool IsDevelopment()
        {
            var appSettingsEnvironment = Environment.GetEnvironmentVariable("APPSETTING_Environment");

            var environmentName = !string.IsNullOrWhiteSpace(appSettingsEnvironment)
                ? appSettingsEnvironment
                : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            var isDevelopment = string.Equals(Environments.Development, environmentName);
            
            return isDevelopment;
        }
    }
}

using MatchedLearnerApi.Configuration;
using MatchedLearnerApi.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MatchedLearnerApi.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(typeof(IMatchedLearnerApiConfiguration),
                x => GetInstance<MatchedLearnerApiConfiguration>(configuration, MatchedLearnerApiConfigurationKeys.MatchedLearnerApi));

            return services;
        }

        private static T GetInstance<T>(IConfiguration configuration, string name)
        {
            var configSection = configuration.GetSection(name);
            
            return configSection.Get<T>();
        }
    }
}
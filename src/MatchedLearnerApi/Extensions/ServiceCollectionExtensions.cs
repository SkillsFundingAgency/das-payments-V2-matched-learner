using MatchedLearnerApi.Application;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Application.Repositories;
using MatchedLearnerApi.Configuration;
using MatchedLearnerApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MatchedLearnerApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(typeof(IMatchedLearnerApiConfiguration),
                x => GetInstance<MatchedLearnerApiConfiguration>(configuration, MatchedLearnerApiConfigurationKeys.MatchedLearnerApi));
            
            return services;
        }

        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddTransient<IPaymentsContext, PaymentsContext>(provider =>
            {
                var configuration = provider.GetService(typeof(IMatchedLearnerApiConfiguration)) as IMatchedLearnerApiConfiguration;
                var builder = new DbContextOptionsBuilder();
                builder.UseSqlServer(configuration.DasPaymentsDatabaseConnectionString);
                return new PaymentsContext(builder.Options);
            });
            services.AddTransient<IEmployerIncentivesRepository, EmployerIncentivesRepository>();
            services.AddTransient<IMatchedLearnerResultMapper, MatchedLearnerResultMapper>();

            return services;
        }

        private static T GetInstance<T>(IConfiguration configuration, string name)
        {
            var configSection = configuration.GetSection(name);
            
            return configSection.Get<T>();
        }
    }
}
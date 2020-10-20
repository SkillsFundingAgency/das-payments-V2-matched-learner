using MatchedLearnerApi.Application;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Application.Repositories;
using MatchedLearnerApi.Configuration;
using MatchedLearnerApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MatchedLearnerApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MatchedLearnerApiConfiguration>(configuration.GetSection("MatchedLearner"));
            services.AddSingleton(typeof(IMatchedLearnerApiConfiguration), cfg => 
                cfg.GetService<IOptions<MatchedLearnerApiConfiguration>>().Value);
            
            return services;
        }

        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddTransient<IPaymentsContext, PaymentsContext>(provider =>
            {
                var configuration = provider.GetService<IMatchedLearnerApiConfiguration>();
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
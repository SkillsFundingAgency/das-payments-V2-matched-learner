using System;
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
            var matchedLearnerConfig = configuration
                .GetSection(MatchedLearnerApiConfigurationKeys.MatchedLearnerConfigKey)
                .Get<MatchedLearnerApiConfiguration>();

            if (matchedLearnerConfig == null)
                throw new InvalidOperationException("invalid Configuration, unable find 'MatchedLearner' Configuration section");

            services.AddSingleton<IMatchedLearnerApiConfiguration>(matchedLearnerConfig);

            return services;
        }

        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddTransient<IPaymentsContext, PaymentsContext>(provider =>
            {
                var configuration = provider.GetService<IMatchedLearnerApiConfiguration>();
                if (configuration == null)
                {
                    throw new InvalidOperationException($"invalid Configuration, unable create instance of {MatchedLearnerApiConfigurationKeys.MatchedLearnerConfigKey}");
                }
                var builder = new DbContextOptionsBuilder();
                builder.UseSqlServer(configuration.DasPaymentsDatabaseConnectionString);
                return new PaymentsContext(builder.Options);
            });
            services.AddTransient<IEmployerIncentivesRepository, EmployerIncentivesRepository>();
            services.AddTransient<IMatchedLearnerResultMapper, MatchedLearnerResultMapper>();

            return services;
        }
    }
}
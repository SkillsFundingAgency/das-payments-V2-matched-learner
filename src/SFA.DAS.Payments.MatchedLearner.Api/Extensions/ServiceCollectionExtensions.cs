using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.MatchedLearner.Api.Configuration;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Data;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Application.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Api.Extensions
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
            services.AddTransient<IPaymentsDataLockRepository, PaymentsDataLockRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();
            services.AddTransient<IMatchedLearnerService, MatchedLearnerService>();

            return services;
        }
    }
}
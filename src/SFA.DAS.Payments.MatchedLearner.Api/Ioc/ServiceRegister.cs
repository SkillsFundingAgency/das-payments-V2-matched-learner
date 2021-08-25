using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Api.Ioc
{
    public static class ServiceRegister
    {

        public static IServiceCollection AddAppDependencies(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddMemoryCache();

            services.AddSingleton<ISqlAzureIdentityTokenProvider, SqlAzureIdentityTokenProvider>();

            services.Decorate<ISqlAzureIdentityTokenProvider, CacheSqlAzureIdentityTokenProvider>();

            services.AddSingleton<SqlAzureIdentityAuthenticationDbConnectionInterceptor>();

            services.AddTransient<IMatchedLearnerDataContextFactory>(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(applicationSettings.MatchedLearnerConnectionString)
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContextFactory(matchedLearnerOptions);
            });

            services.AddTransient(provider =>
            {
                var matchedLearnerOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(applicationSettings.MatchedLearnerConnectionString)
                    .AddInterceptors(provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new MatchedLearnerDataContext(matchedLearnerOptions);
            });

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();
            services.AddTransient<IMatchedLearnerService, MatchedLearnerService>();

            return services;
        }
    }
}
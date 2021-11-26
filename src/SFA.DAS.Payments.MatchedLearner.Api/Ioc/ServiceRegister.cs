using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

namespace SFA.DAS.Payments.MatchedLearner.Api.Ioc
{
    public static class ServiceRegister
    {
        public static void AddAppDependencies(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddMatchedLearnerDataContext(applicationSettings);

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();

            services.AddTransient<IMatchedLearnerService>(provider => new MatchedLearnerService(
                provider.GetService<IMatchedLearnerRepository>(),
                provider.GetService<IMatchedLearnerDtoMapper>(),
                provider.GetService<ILegacyMatchedLearnerRepository>(),
                provider.GetService<ILegacyMatchedLearnerDtoMapper>(),
                provider.GetService<ILogger<MatchedLearnerService>>(),
                applicationSettings.UseV1Api
            ));

            services.AddTransient<ILegacyMatchedLearnerRepository, LegacyMatchedLearnerRepository>();
            services.AddTransient<ILegacyMatchedLearnerDtoMapper, LegacyMatchedLearnerDtoMapper>();
        }
    }
}
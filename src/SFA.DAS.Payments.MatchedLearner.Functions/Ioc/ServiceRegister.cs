using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Ioc
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddTransient<IMatchedLearnerContext, MatchedLearnerContext>(provider =>
            {
                var applicationSettings = ServiceCollectionExtensions.GetApplicationSettings(null, provider);

                var builder = new DbContextOptionsBuilder();
                builder.UseSqlServer(applicationSettings.PaymentsConnectionString);
                return new MatchedLearnerContext(builder.Options);
            });
            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();
            services.AddTransient<IMatchedLearnerService, MatchedLearnerService>();

            return services;
        }
    }
}
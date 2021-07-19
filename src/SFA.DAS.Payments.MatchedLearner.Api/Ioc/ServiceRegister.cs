using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

namespace SFA.DAS.Payments.MatchedLearner.Api.Ioc
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            var applicationSettings = services.GetApplicationSettings();

            var dbContextOptions = new DbContextOptionsBuilder()
                .UseSqlServer(applicationSettings.MatchedLearnerConnectionString)
                .Options;

            services.AddTransient<IMatchedLearnerDataContextFactory, MatchedLearnerDataContextFactory>(provider => 
                new MatchedLearnerDataContextFactory(dbContextOptions));

            services.AddTransient<MatchedLearnerDataContext, MatchedLearnerDataContext>(provider => 
                new MatchedLearnerDataContext(dbContextOptions));

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();
            services.AddTransient<IMatchedLearnerService, MatchedLearnerService>();

            return services;
        }
    }
}
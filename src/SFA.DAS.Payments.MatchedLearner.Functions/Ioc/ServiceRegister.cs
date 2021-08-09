using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Ioc
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddAppDependencies(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            var dbContextOptions = new DbContextOptionsBuilder()
                .UseSqlServer(applicationSettings.MatchedLearnerConnectionString)
                .Options;

            services.AddTransient<IMatchedLearnerDataContextFactory, MatchedLearnerDataContextFactory>(provider => 
                new MatchedLearnerDataContextFactory(dbContextOptions));

            services.AddTransient<MatchedLearnerDataContext, MatchedLearnerDataContext>(provider => 
                new MatchedLearnerDataContext(dbContextOptions));

            services.AddTransient<IPaymentsDataContext, PaymentsDataContext>(provider =>
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(applicationSettings.PaymentsConnectionString)
                    .Options;

                return new PaymentsDataContext(options);
            });

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IPaymentsRepository, PaymentsRepository>();
            services.AddTransient<IMatchedLearnerDataImportService, MatchedLearnerDataImportService>();

            return services;
        }
    }
}
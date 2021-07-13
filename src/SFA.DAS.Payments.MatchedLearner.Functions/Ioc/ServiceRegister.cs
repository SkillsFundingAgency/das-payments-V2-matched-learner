using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.MatchedLearner.Application;
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
                builder.UseSqlServer(applicationSettings.MatchedLearnerConnectionString);
                return new MatchedLearnerContext(builder.Options);
            });

            services.AddTransient<IMatchedLearnerDataContextFactory, MatchedLearnerDataContextFactory>(x =>

            {
                var applicationSettings = ServiceCollectionExtensions.GetApplicationSettings(null, x);

                var builder = new DbContextOptionsBuilder();
                builder.UseSqlServer(applicationSettings.MatchedLearnerConnectionString);

                return new MatchedLearnerDataContextFactory(builder);
            });

            services.AddTransient<IDataLockEventDataContext, DataLockEventDataContext>(provider =>
            {
                var applicationSettings = ServiceCollectionExtensions.GetApplicationSettings(null, provider);

                var builder = new DbContextOptionsBuilder();
                builder.UseSqlServer(applicationSettings.PaymentsConnectionString);

                return new DataLockEventDataContext(builder.Options);
            });

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IDataLockEventRepository, DataLockEventRepository>();
            services.AddTransient<IMatchedLearnerDataImportService, MatchedLearnerDataImportService>();

            return services;
        }
    }
}
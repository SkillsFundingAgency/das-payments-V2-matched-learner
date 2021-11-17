using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Ioc
{
    public static class ServiceRegister
    {
        public static void AddAppDependencies(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            services.AddMatchedLearnerDataContext(applicationSettings);

            services.AddPaymentsDataContext(applicationSettings);

            services.AddEndpointInstanceFactory(applicationSettings);

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IPaymentsRepository, PaymentsRepository>();
            services.AddTransient<IMatchedLearnerDataImportService, MatchedLearnerDataImportService>();

            services.AddTransient<IMatchedLearnerMigrationService, MatchedLearnerMigrationService>(x => 
                new MatchedLearnerMigrationService(
                    x.GetService<MatchedLearnerDataContext>(), 
                    x.GetService<IEndpointInstanceFactory>(), 
                    applicationSettings.MigrationQueue));

            services.AddTransient<IProviderLevelMatchedLearnerMigrationService, ProviderLevelMatchedLearnerMigrationService>();
            services.AddTransient<IProviderMigrationRepository, ProviderMigrationRepository>();
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
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

            services.AddTransient<IMatchedLearnerDataImporter, MatchedLearnerDataImporter>();
            services.AddTransient<ILegacyMatchedLearnerRepository, LegacyMatchedLearnerRepository>();
            services.AddTransient<ILegacyMatchedLearnerDataImportService, LegacyMatchedLearnerDataImportService>();

            services.AddTransient<IMatchedLearnerMigrationService, MatchedLearnerMigrationService>(x => 
                new MatchedLearnerMigrationService(
                    x.GetService<MatchedLearnerDataContext>(), 
                    x.GetService<IEndpointInstanceFactory>(), 
                    applicationSettings.MigrationQueue,
                    x.GetService<IProviderMigrationRepository>()));

            services.AddTransient<IProviderLevelMatchedLearnerMigrationService, ProviderLevelMatchedLearnerMigrationService>(x =>
                new ProviderLevelMatchedLearnerMigrationService(
                    x.GetService<IProviderMigrationRepository>(),
                    x.GetService<IMatchedLearnerRepository>(),
                    x.GetService<IMatchedLearnerDtoMapper>(),
                    x.GetService<ILogger<ProviderLevelMatchedLearnerMigrationService>>(),
                    applicationSettings.MigrationBatchSize,
                    x.GetService<IEndpointInstanceFactory>(),
                    applicationSettings.MigrationQueue));
            services.AddTransient<IProviderMigrationRepository, ProviderMigrationRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();
        }
    }
}
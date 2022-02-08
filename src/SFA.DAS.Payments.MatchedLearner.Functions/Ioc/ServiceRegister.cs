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

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IPaymentsRepository, PaymentsRepository>();
            services.AddTransient<IMatchedLearnerDataImportService, MatchedLearnerDataImportService>();

            services.AddTransient<IMatchedLearnerDataImporter, MatchedLearnerDataImporter>();
            services.AddTransient<ILegacyMatchedLearnerRepository, LegacyMatchedLearnerRepository>();
            services.AddTransient<ILegacyMatchedLearnerDataImportService, LegacyMatchedLearnerDataImportService>();

            services.AddTransient<ISubmissionSucceededDelayedImportService, SubmissionSucceededDelayedImportService>(x =>
                new SubmissionSucceededDelayedImportService(applicationSettings,
                    x.GetService<ILogger<SubmissionSucceededDelayedImportService>>()));

            services.AddTransient<IMigrateProviderMatchedLearnerDataTriggerService, MigrateProviderMatchedLearnerDataTriggerService>(x =>
                new MigrateProviderMatchedLearnerDataTriggerService(x.GetService<MatchedLearnerDataContext>(),
                    x.GetService<IProviderMigrationRepository>(),
                    x.GetService<ILogger<MigrateProviderMatchedLearnerDataTriggerService>>()));

            services.AddTransient<IMigrateProviderMatchedLearnerDataService, MigrateProviderMatchedLearnerDataService>(x =>
                new MigrateProviderMatchedLearnerDataService(applicationSettings,
                    x.GetService<IProviderMigrationRepository>(),
                    x.GetService<IMatchedLearnerRepository>(),
                    x.GetService<IMatchedLearnerDtoMapper>(),
                    x.GetService<ILogger<MigrateProviderMatchedLearnerDataService>>()));
            services.AddTransient<IProviderMigrationRepository, ProviderMigrationRepository>();
            services.AddTransient<IMatchedLearnerDtoMapper, MatchedLearnerDtoMapper>();
        }
    }
}
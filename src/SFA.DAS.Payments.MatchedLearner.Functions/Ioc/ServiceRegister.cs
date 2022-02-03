using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Application;
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

            services.AddEndpointInstance(applicationSettings);

            services.AddTransient<IMatchedLearnerRepository, MatchedLearnerRepository>();
            services.AddTransient<IPaymentsRepository, PaymentsRepository>();
            services.AddTransient<IMatchedLearnerDataImportService, MatchedLearnerDataImportService>();

            services.AddTransient<ISubmissionSucceededDelayedImportService, SubmissionSucceededDelayedImportService>(x =>
                new SubmissionSucceededDelayedImportService(applicationSettings,
                    x.GetService<IEndpointInstance>(),
                    x.GetService<ILogger<SubmissionSucceededDelayedImportService>>()));

        }
    }
}
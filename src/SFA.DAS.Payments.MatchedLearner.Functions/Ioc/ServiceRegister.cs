using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
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
        private static readonly AzureServiceTokenProvider AzureServiceTokenProvider = new AzureServiceTokenProvider();

        public static IServiceCollection AddAppDependencies(this IServiceCollection services, ApplicationSettings applicationSettings)
        {
            var connection = new SqlConnection
            {
                ConnectionString = applicationSettings.MatchedLearnerConnectionString,
                AccessToken = AzureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult()
            };

            var matchedLearnerOptions = new DbContextOptionsBuilder()
                .UseSqlServer(connection)
                .Options;


            services.AddTransient<IMatchedLearnerDataContextFactory, MatchedLearnerDataContextFactory>(provider =>
                new MatchedLearnerDataContextFactory(matchedLearnerOptions));

            services.AddTransient<MatchedLearnerDataContext, MatchedLearnerDataContext>(provider =>
                new MatchedLearnerDataContext(matchedLearnerOptions));

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
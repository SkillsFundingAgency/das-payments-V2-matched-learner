using MatchedLearnerApi.Application;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MatchedLearnerApi.IoC
{
    public static class MatchedLearnerApiRegistry
    {
        public static void AddMatchedLearner(this IServiceCollection services)
        {
            services.AddTransient<IPaymentsContext, PaymentsContext>(provider =>
            {
                var configuration = provider.GetService(typeof(IConfiguration)) as IConfiguration;
                var connectionString = configuration.GetConnectionString("PaymentsConnectionString");
                var builder = new DbContextOptionsBuilder();
                builder.UseSqlServer(connectionString);
                return new PaymentsContext(builder.Options);
            });
            services.AddTransient<IEmployerIncentivesRepository, EmployerIncentivesRepository>();
            services.AddTransient<IMatchedLearnerResultMapper, MatchedLearnerResultMapper>();
        }
    }
}

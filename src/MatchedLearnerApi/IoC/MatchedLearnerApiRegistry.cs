using MatchedLearnerApi.Application;
using MatchedLearnerApi.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MatchedLearnerApi.IoC
{
    public class MatchedLearnerApiRegistry
    {
        public void Register(IServiceCollection services)
        {
            services.AddTransient<IPaymentsContext, PaymentsContext>();
            services.AddTransient<IEmployerIncentivesRepository, EmployerIncentivesRepository>();
            services.AddTransient<IMatchedLearnerResultMapper, MatchedLearnerResultMapper>();
        }
    }
}

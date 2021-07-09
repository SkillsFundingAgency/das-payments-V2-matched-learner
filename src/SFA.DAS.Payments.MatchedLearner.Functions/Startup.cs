using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Extensions;

[assembly: FunctionsStartup(typeof(SFA.DAS.Payments.MatchedLearner.Functions.Startup))]
namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var config = configuration.InitialiseConfigure();

            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddApiConfigurationSections(config);

            builder.Services.AddNLog(config);

            builder.Services.AddOptions();

            builder.Services.AddAppDependencies();
        }
    }
}

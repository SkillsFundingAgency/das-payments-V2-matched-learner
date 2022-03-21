using System;
using System.Net.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.EI.TestClient;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.Payments.EI.TestClient
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            
            builder.Services.AddOptions();

            var configuration = builder.GetContext().Configuration;

            var connectionString = configuration["ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ApplicationException("Configuration is not initialized correctly");

            builder.Services.AddTransient(_ =>
            {
                var options = new DbContextOptionsBuilder()
                    .UseSqlServer(new SqlConnection(connectionString), optionsBuilder => optionsBuilder.CommandTimeout(540))
                    .Options;

                return new TestDataContext(options);
            });

            
            var matchedLearnerApiBaseUrl = configuration["matchedLearnerApiBaseUrl"];

            if (string.IsNullOrEmpty(matchedLearnerApiBaseUrl))
                throw new ApplicationException("Configuration is not initialized correctly");

            builder.Services.AddTransient(_ =>
            {
                var httpClient = new HttpClient();
                
                httpClient.BaseAddress = new Uri(matchedLearnerApiBaseUrl);;
                
                return httpClient;
            });
        }
    }
}

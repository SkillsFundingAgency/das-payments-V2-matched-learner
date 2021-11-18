using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestFunctionHost : IDisposable
    {
        private readonly IHost _host;
        private bool _isDisposed;

        public TestFunctionHost()
        {
            var appConfig = new Dictionary<string, string>{
                { "EnvironmentName", "Development" },
                { "AzureWebJobsStorage", TestConfiguration.TestApplicationSettings.MatchedLearnerStorageAccountConnectionString },
                { "PaymentsServiceBusConnectionString", TestConfiguration.TestApplicationSettings.PaymentsServiceBusConnectionString },
                { "MatchedLearnerQueue", TestConfiguration.TestApplicationSettings.MatchedLearnerQueue },
                { "MigrationQueue", TestConfiguration.TestApplicationSettings.MigrationQueue },
            };

            var directory = Directory.GetCurrentDirectory();

            var inMemoryConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddInMemoryCollection(appConfig)
                .Build();

            var context = new WebJobsBuilderContext
            {
                ApplicationRootPath = directory[..directory.IndexOf("bin", StringComparison.Ordinal)],
                Configuration = inMemoryConfig,
                EnvironmentName = "Development"
            };

            _host = new HostBuilder()
                .ConfigureAppConfiguration(a =>
                {
                    a.AddInMemoryCollection(appConfig);
                })
                .ConfigureWebJobs(builder => builder
                    .AddAzureStorageCoreServices()
                    .AddServiceBus()
                    .UseWebJobsStartup(typeof(Startup), context, NullLoggerFactory.Instance))
                .Build();
        }

        public async Task StartHost()
        {
            var timeout = new TimeSpan(0, 0, 10);
            var delayTask = Task.Delay(timeout);
            await Task.WhenAny(Task.WhenAll(_host.StartAsync()), delayTask);

            if (delayTask.IsCompleted)
            {
                throw new Exception($"Failed to start test function host within {timeout.Seconds} seconds.  Check the AzureStorageEmulator is running. ");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _host.Dispose();
            }

            _isDisposed = true;
        }
    }
}
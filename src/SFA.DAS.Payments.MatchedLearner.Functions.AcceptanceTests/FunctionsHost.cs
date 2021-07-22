using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    [Binding]
    public class FunctionsHost
    {
        private readonly SmokeTestContext _testContext;
        private readonly FeatureContext _featureContext;
        public FunctionsHost(SmokeTestContext testContext, FeatureContext featureContext)
        {
            _testContext = testContext;
            _featureContext = featureContext;
        }

        [BeforeScenario()]
        public async Task InitialiseHost()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _testContext.TestFunction = new TestFunction($"TEST{_featureContext.FeatureInfo.Title}");
            await _testContext.TestFunction.StartHost();
            stopwatch.Stop();
            Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
        }

        [AfterScenario()]
        public void Cleanup()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _testContext.TestFunction.Dispose();
            stopwatch.Stop();
            Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
        }
    }

    public class TestFunction : IDisposable
    {
        private readonly IHost _host;
        private bool _isDisposed;

        public string HubName { get; }
        public TestFunction(string hubName)
        {
            HubName = hubName;

            var fileConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: false)
                .Build();

            var appConfig = new Dictionary<string, string>{
                { "EnvironmentName", "Development" },
                { "AzureWebJobsStorage", "UseDevelopmentStorage=true" },
                { "MatchedLearnerQueue", fileConfig.GetValue<string>("Values:MatchedLearnerQueue") },
                { "MatchedLearnerServiceBusConnectionString",  fileConfig.GetValue<string>("Values:MatchedLearnerServiceBusConnectionString")},
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
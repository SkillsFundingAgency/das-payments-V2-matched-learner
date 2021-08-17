using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class TestBindingBase
    {
        public readonly TestContext TestContext;
        public readonly FeatureContext FeatureContext;

        public TestBindingBase(TestContext testContext, FeatureContext featureContext)
        {
            TestContext = testContext;
            FeatureContext = featureContext;
        }

        [BeforeScenario]
        public async Task Initialise()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //This is a Hack to check if the Tests are Running on Local Machine
            if (string.IsNullOrEmpty(TestConfiguration.TestApplicationSettings.TargetUrl))
            {
                TestContext.TestFunctionHost = new TestFunctionHost();
                await TestContext.TestFunctionHost.StartHost();
            }

            TestContext.TestEndpointInstance = new TestEndpoint();
            await TestContext.TestEndpointInstance.Start();

            TestContext.TestRepository = new TestRepository();

            TestContext.TimeToPause = TimeSpan.Parse(TestConfiguration.TestApplicationSettings.TimeToPause);

            TestContext.TimeToWait = TimeSpan.Parse(TestConfiguration.TestApplicationSettings.TimeToWait);

            TestContext.TimeToWaitUnexpected = TimeSpan.Parse(TestConfiguration.TestApplicationSettings.TimeToWaitUnexpected);

            stopwatch.Stop();

            Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds");
        }

        [AfterScenario]
        public async Task Cleanup()
        {
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //This is a Hack to check if the Tests are Running on Local Machine
            if (string.IsNullOrEmpty(TestConfiguration.TestApplicationSettings.TargetUrl))
            {
                TestContext.TestFunctionHost.Dispose();
            }

            TestContext.TestRepository.Dispose();

            await TestContext.TestEndpointInstance.Stop();

            stopwatch.Stop();
            Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds");
        }
    }
}
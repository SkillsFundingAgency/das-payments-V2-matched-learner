using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class TestBindingBase
    {
        public string Url;

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
            //This is a Hack to check if the Tests are Running on Local Machine
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Url = TestConfiguration.ApplicationSettings.TargetUrl;

            if (string.IsNullOrEmpty(Url))
            {
                TestContext.TestFunctionHost = new TestFunctionHost();
                await TestContext.TestFunctionHost.StartHost();
            }

            TestContext.TestEndpointInstance = new TestEndpoint();
            await TestContext.TestEndpointInstance.Start();

            TestContext.TestRepository = new TestRepository();

            TestContext.TimeToPause = TimeSpan.Parse(TestConfiguration.ApplicationSettings.TimeToPause);

            TestContext.TimeToWait = TimeSpan.Parse(TestConfiguration.ApplicationSettings.TimeToWait);

            TestContext.TimeToWaitUnexpected = TimeSpan.Parse(TestConfiguration.ApplicationSettings.TimeToWaitUnexpected);

            stopwatch.Stop();

            Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds");
        }

        [AfterScenario]
        public async Task Cleanup()
        {
            //This is a Hack to check if the Tests are Running on Local Machine
            Url = TestConfiguration.ApplicationSettings.TargetUrl;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (string.IsNullOrEmpty(Url))
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
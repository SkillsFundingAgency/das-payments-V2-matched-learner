using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class TestBindingBase
    {
        public readonly TestContext TestContext;

        public TestBindingBase(TestContext testContext)
        {
            TestContext = testContext;
        }

        public async Task WaitForIt(Func<Task<bool>> lookForIt, string failText)
        {
            var endTime = DateTime.Now.Add(TestConfiguration.TestApplicationSettings.TimeToWait);
            var lastRun = false;

            while (DateTime.Now < endTime || lastRun)
            {
                if (await lookForIt())
                {
                    if (lastRun) return;
                    lastRun = true;
                }
                else
                {
                    if (lastRun) break;
                }

                await Task.Delay(TestConfiguration.TestApplicationSettings.TimeToPause);
            }

            Assert.Fail($"{failText}  Time: {DateTime.Now:G}.  Ukprn: {TestContext.Ukprn}.");
        }

        protected async Task WaitForUnexpected(Func<Task<bool>> findUnexpected, string failText)
        {
            var endTime = DateTime.Now.Add(TestConfiguration.TestApplicationSettings.TimeToWaitUnexpected);
            while (DateTime.Now < endTime)
            {
                if (! await findUnexpected())
                {
                    Assert.Fail($"{failText} Time: {DateTime.Now:G}.  Ukprn: {TestContext.Ukprn}.");
                }

                await Task.Delay(TestConfiguration.TestApplicationSettings.TimeToPause);
            }
        }
    }

    [Binding]
    public class ScenarioBase
    {
        public readonly TestContext TestContext;

        public ScenarioBase(TestContext testContext)
        {
            TestContext = testContext;
        }

        [BeforeScenario]
        public async Task Initialise()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //This is a Hack to check if the Tests are Running on Local Machine
            if (string.IsNullOrEmpty(TestConfiguration.TestAzureAdClientSettings.ApiBaseUrl))
            {
                Console.WriteLine("Starting Functions Host");

                TestContext.TestFunctionHost = new TestFunctionHost();
                await TestContext.TestFunctionHost.StartHost();
            }

            Console.WriteLine("Starting Test Endpoint");

            TestContext.TestEndpointInstance = new TestEndpoint();
            await TestContext.TestEndpointInstance.Start();

            TestContext.TestRepository = new TestRepository();

            stopwatch.Stop();

            Console.WriteLine($"Time it took to Initialise TestContext: {stopwatch.Elapsed.Milliseconds} milliseconds");
        }

        [AfterScenario]
        public async Task Cleanup()
        {

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //This is a Hack to check if the Tests are Running on Local Machine
            if (string.IsNullOrEmpty(TestConfiguration.TestAzureAdClientSettings.ApiBaseUrl) && TestContext.TestFunctionHost != null)
            {
                TestContext.TestFunctionHost.Dispose();
            }

            TestContext.TestRepository.Dispose();

            if (TestContext.TestEndpointInstance != null) await TestContext.TestEndpointInstance.Stop();

            stopwatch.Stop();
            Console.WriteLine($"Time it took to Cleanup  TestContext: {stopwatch.Elapsed.Milliseconds} milliseconds");
        }
    }
}
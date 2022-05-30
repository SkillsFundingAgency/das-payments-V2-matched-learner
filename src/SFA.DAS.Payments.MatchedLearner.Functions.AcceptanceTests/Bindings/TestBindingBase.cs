using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class TestBindingBase
    {
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

            Assert.Fail($"{failText}  Time: {DateTime.Now:G}.");
        }

        protected async Task WaitForUnexpected(Func<Task<bool>> findUnexpected, string failText)
        {
            var endTime = DateTime.Now.Add(TestConfiguration.TestApplicationSettings.TimeToWaitUnexpected);
            while (DateTime.Now < endTime)
            {
                if (! await findUnexpected())
                {
                    Assert.Fail($"{failText} Time: {DateTime.Now:G}.");
                }

                await Task.Delay(TestConfiguration.TestApplicationSettings.TimeToPause);
            }
        }
    }
}
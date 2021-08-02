using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class TestBindingSteps
    {
        private readonly TestContext _testContext;

        public TestBindingSteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given("A Submission Job Succeeded")]
        public async Task GivenASuccessfulSubmissionIsCompleted()
        {
            await _testContext.TestRepository.ClearDataLockEvent(1000, 2000);
            await _testContext.TestRepository.AddDataLockEvent(1000, 2000);
        }

        [When("A SubmissionJobSucceeded message is received")]
        public async Task WhenWeReceiveSubmissionSucceededEvent()
        {
            await _testContext.TestEndpointInstance.PublishSubmissionSucceededEvent(1000, 2021, 1);
        }

        
        [Then("the matched Learners are Imported")]
        public async Task ThenTheMatchedLearnersAreImported()
        {
            await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(1000, 2021, 1);
            await _testContext.TestRepository.ClearDataLockEvent(1000, 2000);
        }
    }
}

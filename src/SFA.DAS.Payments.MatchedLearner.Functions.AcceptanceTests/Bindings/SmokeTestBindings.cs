using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class SmokeTestBindings
    {
        private readonly SmokeTestContext _context;

        public SmokeTestBindings(SmokeTestContext context)
        {
            _context = context;
        }

        [Then(@"the matched Learners are Imported")]
        public void ThenTheMatchedLearnersAreImported()
        {

        }

        [When(@"we receive Submission Succeeded Event")]
        public void WhenWeReceiveSubmissionSucceededEvent()
        {

        }

        [Given(@"A successful submission is completed")]
        public void GivenASuccessfulSubmissionIsCompleted()
        {

        }
    }
}

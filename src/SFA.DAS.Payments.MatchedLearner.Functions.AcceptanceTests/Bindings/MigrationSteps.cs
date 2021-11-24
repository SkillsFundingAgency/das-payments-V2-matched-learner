using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class MigrationSteps
    {
        private readonly TestContext _testContext;
        private readonly long _ukprn;
        private readonly long _learnerUln;
        private readonly long _apprenticeshipId;

        public MigrationSteps(TestContext testContext)
        {
            var random = new Random();
            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);
            _apprenticeshipId = _ukprn + _learnerUln;
            _testContext = testContext;
        }

        [Given(@"a learner has Datalock events in PV(.*) Format")]
        public async Task GivenALearnerHasDatalockEventsInPVFormat(int p0)
        {
            await _testContext.TestRepository.AddDataLockEvent(_ukprn, _learnerUln, 1, 2122, true);
        }
        
        [When(@"Migration is Run")]
        public async Task WhenMigrationIsRun()
        {
            var url = "http://localhost:5000/api/HttpTriggerMatchedLearnerMigration";

            var client = new HttpClient();
            var result = await client.GetAsync(url);
        }

        [Then(@"learner Datalock events are migrated into the new format")]
        public void ThenLearnerDatalockEventsAreMigratedIntoTheNewFormat()
        {
        }

    }
}

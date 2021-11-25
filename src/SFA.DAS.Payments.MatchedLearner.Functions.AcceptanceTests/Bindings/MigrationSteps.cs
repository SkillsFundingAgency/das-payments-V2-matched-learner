using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
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
            var url = "http://localhost:7071/api/HttpTriggerMatchedLearnerMigration";

            var client = new HttpClient();
            var result = await client.GetAsync(url);
        }

        [Then(@"learner Datalock events are migrated into the new format")]
        public async Task ThenLearnerDatalockEventsAreMigratedIntoTheNewFormat()
        {
            var datalockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(_ukprn);
            var expectedDle = datalockEvents.First();

            var trainingRecords = await _testContext.TestRepository.GetMatchedLearnerTrainings(_ukprn);

            trainingRecords.Count.Should().Be(1);

            var training = trainingRecords.First();

            training.Ukprn.Should().Be(expectedDle.Ukprn);
            training.AcademicYear.Should().Be(expectedDle.AcademicYear);
            training.IlrSubmissionDate.Should().Be(expectedDle.IlrSubmissionDateTime);
            training.CompletionStatus.Should().Be(expectedDle.CompletionStatus);
            training.FrameworkCode.Should().Be(expectedDle.LearningAimFrameworkCode);
            training.PathwayCode.Should().Be(expectedDle.LearningAimPathwayCode);
            training.StandardCode.Should().Be(expectedDle.LearningAimStandardCode);
            training.ProgrammeType.Should().Be(expectedDle.LearningAimProgrammeType);
            training.StartDate.Should().Be(expectedDle.LearningStartDate);
            training.CompletionStatus.Should().Be(expectedDle.CompletionStatus);
            training.Reference.Should().Be(expectedDle.LearningAimReference);
        }
    }
}

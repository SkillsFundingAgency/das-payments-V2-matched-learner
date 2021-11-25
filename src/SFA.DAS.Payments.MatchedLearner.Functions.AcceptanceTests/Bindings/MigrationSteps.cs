using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class MigrationSteps
    {
        private readonly TestContext _testContext;
        private readonly long _ukprn;
        private long _learnerUln;
        private int _learnerCount;

        private readonly List<long> _listOfUln = new List<long>();

        public MigrationSteps(TestContext testContext)
        {
            var random = new Random();
            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);

            _testContext = testContext;
        }

        [Given("A Successful Submission Job for (.*) Learners in CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task GivenASuccessfulSubmissionForMultipleLearnersInPeriod(int learnerCount, byte collectionPeriod, short academicYear)
        {
            _learnerCount = learnerCount;

            for (var i = 1; i < learnerCount; i++)
            {
                _listOfUln.Add(_learnerUln++);
                await _testContext.TestRepository.ClearMatchedLearnerTrainings(_ukprn, _learnerUln);
                await _testContext.TestRepository.ClearDataLockEvent(_ukprn, _learnerUln);
                await _testContext.TestRepository.AddDataLockEvent(_ukprn, _learnerUln, collectionPeriod, academicYear, false);
            }
        }

        [Given("Duplicate Matched Learners Trainings Already Exists for (.*) Learners CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task ADuplicateRecordIsLeUnDeletedFromPreviousRun(int learnerCount, byte collectionPeriod, short academicYear)
        {
            var random = new Random();

            for (var i = 1; i < learnerCount; i++)
            {
                var index = random.Next(_learnerCount);
                var learnerUln = _listOfUln[index];

                await _testContext.TestRepository.AddMatchedLearnerTrainings(_ukprn, learnerUln, collectionPeriod, academicYear);
            }
        }

        [When("Migration is Run for Provider")]
        public async Task WhenMigrationIsRunForProvider()
        {
            await _testContext.TestEndpointInstance.PublishProviderLevelMigrationRequest(_ukprn);
        }

        [Then("Matched Learners Trainings are only Imported for (.*) Learners")]
        public async Task ThenLearnerDataLockEventsAreMigratedIntoTheNewFormat(int learnerCount)
        {
            var dataLockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(_ukprn);
            var expectedDle = dataLockEvents.First();

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

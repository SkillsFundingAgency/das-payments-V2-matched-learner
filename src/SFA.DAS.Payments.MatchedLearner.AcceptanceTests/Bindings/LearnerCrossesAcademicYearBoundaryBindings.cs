using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Bindings
{
    [Binding]
    public class LearnerCrossesAcademicYearBoundaryBindings
    {
        private readonly SmokeTestContext _textContext;

        private readonly long _ukprn;
        private readonly long _learnerUln;
        private readonly long _apprenticeshipId;
        private readonly TestRepository _repository;

        public LearnerCrossesAcademicYearBoundaryBindings(SmokeTestContext textContext)
        {
            _textContext = textContext;

            var random = new Random();

            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);
            _apprenticeshipId = _ukprn + _learnerUln;
            _repository = new TestRepository();
        }

        [Given(@"the provider submitted a learner in Academic Year (.*) and Collection Period (.*)")]
        public async Task GivenTheProviderSubmittedALearnerInAcademicYearAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            await _repository.ClearTestData(_ukprn, _learnerUln);

            var dataLockEventId = Guid.NewGuid();

            await _repository.AddDataLockEventForAcademicYear(_ukprn, _learnerUln, academicYear, collectionPeriod, dataLockEventId);

            var submissionJob = await _repository.AddProviderSubmissionJob(academicYear, collectionPeriod, _ukprn);

            _textContext.ProviderSubmissions.Add(submissionJob);
        }

        [Given(@"the learner then had a break in learning")]
        public void GivenTheLearnerThenHadABreakInLearning()
        {
        }

        [Given(@"the provider then submitted again in Academic Year (.*) and Collection Period (.*)")]
        public async Task GivenTheProviderThenSubmittedAgainInAcademicYearAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            var submissionJob = await _repository.AddProviderSubmissionJob(academicYear, collectionPeriod, _ukprn);

            _textContext.ProviderSubmissions.Add(submissionJob);
        }

        [When(@"the Api is called in AY (.*) and Collection Period (.*)")]
        public async Task WhenTheApiIsCalledInAYAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            var request = new TestClient();

            _textContext.MatchedLearnerDto = await request.Handle(_ukprn, _learnerUln);
        }

        [Then(@"the header should have AY (.*) and Collection Period (.*)")]
        public void ThenTheHeaderShouldHaveAYAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            _textContext.MatchedLearnerDto.AcademicYear.Should().Be(academicYear);
            _textContext.MatchedLearnerDto.IlrSubmissionWindowPeriod.Should().Be(collectionPeriod);
        }

        [Then(@"the latest price episode Academic Year should be (.*)")]
        public void ThenTheLatestPriceEpisodeAcademicYearShouldBe(short academicYear)
        {
            _textContext.MatchedLearnerDto.Training.First().PriceEpisodes.First().AcademicYear.Should().Be(academicYear);
        }

        [Then(@"the latest price episode CollectionPeriod should be (.*)")]
        public void ThenTheLatestPriceEpisodeCollectionPeriodShouldBe(byte collectionPeriod)
        {
            _textContext.MatchedLearnerDto.Training.First().PriceEpisodes.First().CollectionPeriod.Should().Be(collectionPeriod);
        }
    }
}
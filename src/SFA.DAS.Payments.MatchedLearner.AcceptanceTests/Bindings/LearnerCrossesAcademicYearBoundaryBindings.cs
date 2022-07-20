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
        private readonly TestRepository _repository;

        public LearnerCrossesAcademicYearBoundaryBindings(SmokeTestContext textContext)
        {
            _textContext = textContext;

            var random = new Random();

            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);
            _repository = new TestRepository();
        }

        [BeforeScenario]
        [AfterScenario]
        public async Task SetUp()
        {
            await _repository.ClearTestData(_ukprn, _learnerUln);
        }

        [Given(@"the provider submitted a learner in Academic Year (.*) and Collection Period (.*)")]
        public async Task GivenTheProviderSubmittedALearnerInAcademicYearAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            var dataLockEventId = Guid.NewGuid();

            await _repository.AddDataLockEventForAcademicYear(_ukprn, _learnerUln, academicYear, collectionPeriod, dataLockEventId);

            await _repository.AddProviderSubmissionJob(academicYear, collectionPeriod, _ukprn);
        }

        [Given(@"the learner then had a break in learning")]
        public void GivenTheLearnerThenHadABreakInLearning()
        {
        }

        [Given(@"the provider then submitted without the learner in Academic Year (.*) and Collection Period (.*)")]
        public async Task GivenTheProviderThenSubmittedWithoutTheLearnerInAcademicYearAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            await _repository.AddProviderSubmissionJob(academicYear, collectionPeriod, _ukprn);
        }

        [When(@"the Api is called in AY (.*) and Collection Period (.*)")]
        public async Task WhenTheApiIsCalledInAyAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            var request = new TestClient();

            _textContext.MatchedLearnerDto = await request.Handle(_ukprn, _learnerUln);
        }

        [Then(@"the header should have AY (.*) and Collection Period (.*)")]
        public void ThenTheHeaderShouldHaveAyAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            _textContext.MatchedLearnerDto.AcademicYear.Should().Be(academicYear);
            _textContext.MatchedLearnerDto.IlrSubmissionWindowPeriod.Should().Be(collectionPeriod);
        }

        [Then(@"the latest price episode should have Academic Year (.*) and Collection Period (.*)")]
        public void ThenTheLatestPriceEpisodeShouldHaveAcademicYearAndCollectionPeriod(short academicYear, byte collectionPeriod)
        {
            _textContext.MatchedLearnerDto.Training.First().PriceEpisodes.First().AcademicYear.Should().Be(academicYear);
            _textContext.MatchedLearnerDto.Training.First().PriceEpisodes.First().CollectionPeriod.Should().Be(collectionPeriod);
        }

        [Then(@"the price episode from Academic Year (.*) and Collection Period (.*) submission is also returned")]
        public void ThenThePriceEpisodeFromAcademicYearAndCollectionPeriodSubmissionIsAlsoReturned(short academicYear, byte collectionPeriod)
        {
            _textContext.MatchedLearnerDto.Training.First().PriceEpisodes
                .First(x => x.AcademicYear == academicYear && x.CollectionPeriod == collectionPeriod).Should()
                .NotBeNull();
        }
    }
}
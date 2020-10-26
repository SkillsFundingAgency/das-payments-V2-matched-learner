using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using MatchedLearnerApi.Types;
using TechTalk.SpecFlow;

namespace MatchedLearnerApi.AcceptanceTests.Bindings
{
    public class SmokeTestContext
    {
        public Func<Task> FailedRequest { get; set; }
        public MatchedLearnerDto MatchedLearnerDto { get; set; }
    }

    [Binding]
    public class SmokeTestBindings
    {
        private readonly SmokeTestContext _context;

        public SmokeTestBindings(SmokeTestContext context)
        {
            _context = context;
        }

        private const string ApiCallResultKey = "ApiCallResult";

        [When(@"we call the API with a learner that does not exist")]
        public void WhenWeCallTheApiWithALearnerThatDoesNotExist()
        {
            var request = new TestClient();
            var act = request.Awaiting(async x => await x.Handle(0, 0));
            _context.FailedRequest = act;
        }

        [Then(@"the result should be a (.*)")]
        public void ThenTheResultShouldBeA(int p0)
        {
            _context.FailedRequest.Should().Throw<Exception>().WithMessage($"{p0}");
        }

        [Given(@"we have created a sample learner")]
        public void GivenWeHaveCreatedASampleLearner()
        {
            var repository = new TestRepository();
            repository.ClearLearner(-1000, -2000).Wait();
            repository.AddDatalockEvent(-1000, -2000).Wait();
        }

        [When(@"we call the API with the sample learners details")]
        public async Task WhenWeCallTheApiWithTheSampleLearnersDetails()
        {
            var request = new TestClient();
            _context.MatchedLearnerDto = await request.Handle(-1000, -2000);
        }

        [Then(@"the result matches the sample learner")]
        public void ThenTheResultMatchesTheSampleLearner()
        {
            var actual = _context.MatchedLearnerDto;

            actual.Should().NotBeNull();
            
            actual!.StartDate.Should().Be(new DateTime(2020, 10, 9).ToDateTimeOffset(TimeSpan.FromHours(1)));
            actual.IlrSubmissionDate.Should().Be(new DateTime(2020, 10, 10).ToDateTimeOffset(TimeSpan.FromHours(1)));
            actual.IlrSubmissionWindowPeriod.Should().Be(1);
            actual.AcademicYear.Should().Be(2021);
            actual.Ukprn.Should().Be(-1000);
            actual.Uln.Should().Be(-2000);
            actual.Training.Should().HaveCount(1);

            var training = actual.Training.First();
            training.Reference.Should().Be("ZPROG001");
            training.ProgrammeType.Should().Be(100);
            training.StandardCode.Should().Be(200);
            training.FrameworkCode.Should().Be(300);
            training.PathwayCode.Should().Be(400);
            training.FundingLineType.Should().BeNullOrEmpty();
            training.StartDate.Should().Be(new DateTime(2020, 10, 9));
            training.PriceEpisodes.Should().HaveCount(1);

            var priceEpisode = training.PriceEpisodes.First();
            priceEpisode.Identifier.Should().Be("TEST");
            priceEpisode.AgreedPrice.Should().Be(3000);
            priceEpisode.StartDate.Should().Be(new DateTime(2020, 10, 7));
            priceEpisode.EndDate.Should().Be(new DateTime(2020, 10, 12));
            priceEpisode.NumberOfInstalments.Should().Be(12);
            priceEpisode.InstalmentAmount.Should().Be(50);
            priceEpisode.CompletionAmount.Should().Be(550);
            priceEpisode.Periods.Should().HaveCount(7);

            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 1,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 2,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{1, 2, 3},
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 4,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{7},
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 5,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = -123456,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{9},
            });
        }
    }
}

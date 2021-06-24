using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using SFA.DAS.Payments.MatchedLearner.Types;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Bindings
{
    public class SmokeTestContext
    {
        public Func<Task> FailedRequest { get; set; }
        public List<Func<Task>> Requests { get; set; } = new List<Func<Task>>();
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

        [When(@"we call the API with a learner that does not exist")]
        public void WhenWeCallTheApiWithALearnerThatDoesNotExist()
        {
            var request = new TestClient();
            var act = request.Awaiting(client => client.Handle(0, 0));
            _context.FailedRequest = act;
        }

        [Then(@"the result should be a (.*)")]
        public void ThenTheResultShouldBeA(int p0)
        {
            _context.FailedRequest.Should().Throw<Exception>().WithMessage($"{p0}");
        }

        [Given(@"we have created a sample learner")]
        public async Task GivenWeHaveCreatedASampleLearner()
        {
            var repository = new TestRepository();
            await repository.ClearLearner(-1000, -2000);
            await repository.AddDataLockEvent(-1000, -2000);
        }

        [Given(@"we have created (.*) sample learners")]
        public async Task GivenWeHaveCreatedASampleLearner(int learnerCount)
        {
            var repository = new TestRepository();
            for (var index = 1; index < learnerCount + 1; index++)
            {
                await repository.ClearLearner(index, index);
                await repository.AddDataLockEvent(index, index);
            }
        }

        [When(@"we call the API with the sample learners details")]
        public async Task WhenWeCallTheApiWithTheSampleLearnersDetails()
        {
            var request = new TestClient();
            
            _context.MatchedLearnerDto = await request.Handle(-1000, -2000);
        }

        [When(@"we call the API (.*) times with the sample learners details")]
        public void WhenWeCallTheApiTimesWithTheSampleLearnersDetails(int learnerCount)
        {
            var request = new TestClient();
            for (var index = 1; index < learnerCount + 1; index++)
            {
                var currentIndex = index;
                _context.Requests.Add(request.Awaiting(client => client.Handle(currentIndex, currentIndex)));
            }
        }

        [Then(@"the result should not be any exceptions")]
        public void ThenTheResultShouldBeAnyExceptions()
        {
            foreach (var request in _context.Requests)
            {
                request.Should().NotThrow<Exception>();
            }
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
            training.PriceEpisodes.Should().HaveCount(1); //todo update this to test multiple price episodes

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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Bindings
{
    [Binding]
    public class SmokeTestBindings
    {
        private readonly SmokeTestContext _context;

        private readonly long _ukprn;
        private readonly long _learnerUln;
        private readonly long _apprenticeshipId;

        public SmokeTestBindings(SmokeTestContext context)
        {
            _context = context;

            var random = new Random();

            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);
            _apprenticeshipId = _ukprn + _learnerUln;
        }

        [When("we call the API with a learner that does not exist")]
        public void WhenWeCallTheApiWithALearnerThatDoesNotExist()
        {
            var request = new TestClient();
            var act = request.Awaiting(client => client.Handle(0, 0));
            _context.FailedRequest = act;
        }

        [Then("the result should be a (.*)")]
        public void ThenTheResultShouldBeA(int p0)
        {
            _context.FailedRequest.Should().ThrowAsync<Exception>().WithMessage($"{p0}");
        }

        [Given("we have created a sample learner")]
        public async Task GivenWeHaveCreatedASampleLearner()
        {
            var repository = new TestRepository();
            await repository.ClearTestData(_ukprn, _learnerUln);
            await repository.AddDataLockEvent(_ukprn, _learnerUln);
            await repository.AddProviderSubmissionJob(2122, 1, _ukprn);
        }

        [Given("we have created (.*) sample learners")]
        public async Task GivenWeHaveCreatedASampleLearner(int learnerCount)
        {
            var repository = new TestRepository();
            for (var index = 1; index < learnerCount + 1; index++)
            {
                await repository.ClearTestData(index, index);
                await repository.AddDataLockEvent(index, index);
            }
        }

        [When("we call the API with the sample learners details")]
        public async Task WhenWeCallTheApiWithTheSampleLearnersDetails()
        {
            var request = new TestClient();
            
            _context.MatchedLearnerDto = await request.Handle(_ukprn, _learnerUln);
        }

        [When("we call the API (.*) times with the sample learners details")]
        public void WhenWeCallTheApiTimesWithTheSampleLearnersDetails(int learnerCount)
        {
            var request = new TestClient();
            for (var index = 1; index < learnerCount + 1; index++)
            {
                var currentIndex = index;
                _context.Requests.Add(request.Awaiting(client => client.Handle(currentIndex, currentIndex)));
            }
        }

        [Then("the result should not be any exceptions")]
        public void ThenTheResultShouldBeAnyExceptions()
        {
            foreach (var request in _context.Requests)
            {
                request.Should().NotThrowAsync<Exception>();
            }
        }

        [Then("the result matches the sample learner")]
        public void ThenTheResultMatchesTheSampleLearner()
        {
            var actual = _context.MatchedLearnerDto;

            actual.Should().NotBeNull();
            
            actual.StartDate.Date.Should().Be(new DateTime(2020, 10, 9));
            actual.IlrSubmissionDate.Date.Should().Be(new DateTime(2021, 3, 1));
            actual.IlrSubmissionWindowPeriod.Should().Be(1);
            actual.AcademicYear.Should().Be(2122);
            actual.Ukprn.Should().Be(_ukprn);
            actual.Uln.Should().Be(_learnerUln);
            actual.Training.Should().HaveCount(1);

            var training = actual.Training.First();
            training.Reference.Should().Be("ZPROG001");
            training.ProgrammeType.Should().Be(100);
            training.StandardCode.Should().Be(200);
            training.FrameworkCode.Should().Be(300);
            training.PathwayCode.Should().Be(400);
            training.FundingLineType.Should().BeNullOrEmpty();
            training.StartDate.Date.Should().Be(new DateTime(2020, 10, 9));
            training.PriceEpisodes.Should().HaveCount(2);

            var priceEpisode = training.PriceEpisodes.ElementAt(1);
            priceEpisode.Identifier.Should().Be("25-104-01/08/2019");
            priceEpisode.AcademicYear.Should().Be(1920);
            priceEpisode.CollectionPeriod.Should().Be(14);
            priceEpisode.AgreedPrice.Should().Be(3000);
            priceEpisode.StartDate.Date.Should().Be(new DateTime(2019, 08, 01));
            priceEpisode.EndDate?.Date.Should().Be(new DateTime(2020, 10, 12));
            priceEpisode.NumberOfInstalments.Should().Be(12);
            priceEpisode.InstalmentAmount.Should().Be(50);
            priceEpisode.CompletionAmount.Should().Be(550);
            priceEpisode.TotalNegotiatedPriceStartDate?.Date.Should().Be(new DateTime(2021, 01, 01));
            priceEpisode.Periods.Should().HaveCount(3);

            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 1,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 2,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            

            var priceEpisode2 = training.PriceEpisodes.First();
            priceEpisode2.Identifier.Should().Be("25-104-01/08/2020");
            priceEpisode2.AcademicYear.Should().Be(2021);
            priceEpisode2.CollectionPeriod.Should().Be(1);
            priceEpisode2.AgreedPrice.Should().Be(3000);
            priceEpisode2.StartDate.Date.Should().Be(new DateTime(2020, 08, 01));
            priceEpisode2.EndDate?.Date.Should().Be(new DateTime(2020, 10, 12));
            priceEpisode2.NumberOfInstalments.Should().Be(12);
            priceEpisode2.InstalmentAmount.Should().Be(50);
            priceEpisode2.CompletionAmount.Should().Be(550);
            priceEpisode.TotalNegotiatedPriceStartDate?.Date.Should().Be(new DateTime(2021, 01, 01));
            priceEpisode2.Periods.Should().HaveCount(7);

            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 1,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 2,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                IsPayable = true,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
            });
            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{1, 2, 3},
            });
            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 4,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{7},
            });
            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 5,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte>{9},
            });
        }
    }
}

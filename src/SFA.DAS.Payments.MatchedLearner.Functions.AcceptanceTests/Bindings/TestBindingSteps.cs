using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
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
            var timer = new Stopwatch();

            timer.Start();

            var dataLockEvents = new List<DataLockEventModel>();

            while (!dataLockEvents.Any() && timer.Elapsed < _testContext.TimeToWait)
            {
                dataLockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(1000, 2021, 1);
                
                if (!dataLockEvents.Any())
                    Thread.Sleep(_testContext.TimeToPause);
            }

            AssertDataLockEvents(dataLockEvents.FirstOrDefault());

            timer.Stop();

            await _testContext.TestRepository.ClearDataLockEvent(1000, 2000);
        }

        public void AssertDataLockEvents(DataLockEventModel actual)
        {
            actual.Should().NotBeNull();
            
            //actual!.StartDate.Should().Be(new DateTime(2020, 10, 9));
            //actual.IlrSubmissionDateTime.Should().Be(new DateTime(2020, 10, 10));
            //actual.CollectionPeriod.Should().Be(1);
            //actual.AcademicYear.Should().Be(2021);
            //actual.Ukprn.Should().Be(1000);
            //actual.LearnerUln.Should().Be(2000);
            //actual.AcademicYear.Should().Be(1920);
            //actual.CollectionPeriod.Should().Be(14);

            //actual.LearningAimReference.Should().Be("ZPROG001");
            //actual.LearningAimProgrammeType.Should().Be(100);
            //actual.LearningAimStandardCode.Should().Be(200);
            //actual.LearningAimFrameworkCode.Should().Be(300);
            //actual.LearningAimPathwayCode.Should().Be(400);
            //actual.LearningAimFundingLineType.Should().BeNullOrEmpty();
            //actual.StartDate.Should().Be(new DateTime(2020, 10, 9));
            //actual.PriceEpisodes.Should().HaveCount(2);

            //var priceEpisode = actual.PriceEpisodes.First();
            //priceEpisode.PriceEpisodeIdentifier.Should().Be("25-104-01/08/2019");
            //priceEpisode.AgreedPrice.Should().Be(3000);
            //priceEpisode.StartDate.Should().Be(new DateTime(2019, 08, 01));
            //priceEpisode.ActualEndDate.Should().Be(new DateTime(2020, 10, 12));
            //priceEpisode.NumberOfInstalments.Should().Be(12);
            //priceEpisode.InstalmentAmount.Should().Be(50);
            //priceEpisode.CompletionAmount.Should().Be(550);
            //priceEpisode.EffectiveTotalNegotiatedPriceStartDate.Should().Be(new DateTime(2021, 01, 01));
            //actual.Periods.Should().HaveCount(3);

            //priceEpisode.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 1,
            //    IsPayable = true,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //});
            //priceEpisode.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 2,
            //    IsPayable = true,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //});
            //priceEpisode.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 3,
            //    IsPayable = true,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //});
            

            //var priceEpisode2 = training.PriceEpisodes.ElementAt(1);
            //priceEpisode2.Identifier.Should().Be("25-104-01/08/2020");
            //priceEpisode2.AcademicYear.Should().Be(2021);
            //priceEpisode2.CollectionPeriod.Should().Be(1);
            //priceEpisode2.AgreedPrice.Should().Be(3000);
            //priceEpisode2.StartDate.Should().Be(new DateTime(2020, 08, 01));
            //priceEpisode2.EndDate.Should().Be(new DateTime(2020, 10, 12));
            //priceEpisode2.NumberOfInstalments.Should().Be(12);
            //priceEpisode2.InstalmentAmount.Should().Be(50);
            //priceEpisode2.CompletionAmount.Should().Be(550);
            //priceEpisode.TotalNegotiatedPriceStartDate.Should().Be(new DateTime(2021, 01, 01));
            //priceEpisode2.Periods.Should().HaveCount(7);

            //priceEpisode2.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 1,
            //    IsPayable = true,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //});
            //priceEpisode2.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 2,
            //    IsPayable = true,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //});
            //priceEpisode2.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 3,
            //    IsPayable = true,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //});
            //priceEpisode2.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 3,
            //    IsPayable = false,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //    DataLockFailures = new HashSet<byte>{1, 2, 3},
            //});
            //priceEpisode2.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 4,
            //    IsPayable = false,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //    DataLockFailures = new HashSet<byte>{7},
            //});
            //priceEpisode2.Periods.Should().ContainEquivalentOf(new
            //{
            //    Period = 5,
            //    IsPayable = false,
            //    AccountId = 1000,
            //    ApprenticeshipId = 123456,
            //    ApprenticeshipEmployerType = 3,
            //    TransferSenderAccountId = 500,
            //    DataLockFailures = new HashSet<byte>{9},
            //});
        }
    }
}

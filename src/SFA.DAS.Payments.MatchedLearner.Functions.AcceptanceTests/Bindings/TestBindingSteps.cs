using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
            await GivenASuccessfulSubmissionIsCompletedForPeriod(1, 2021);
        }

        [Given("A Submission Job Succeeded for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task GivenASuccessfulSubmissionIsCompletedForPeriod(byte collectionPeriod, short academicYear)
        {
            await _testContext.TestRepository.ClearDataLockEvent(1000, 2000);
            await _testContext.TestRepository.AddDataLockEvent(1000, 2000, collectionPeriod, academicYear);
        }

        [When("A SubmissionJobSucceeded message is received")]
        public async Task WhenWeReceiveSubmissionSucceeded()
        {
            await WhenWeReceiveSubmissionSucceededEventForPeriod(1, 2021);
        }

        [When("A SubmissionJobSucceeded message is received for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task WhenWeReceiveSubmissionSucceededEventForPeriod(byte collectionPeriod, short academicYear)
        {
            await _testContext.TestEndpointInstance.PublishSubmissionSucceededEvent(1000, academicYear, collectionPeriod);
        }

        [Then("the matched Learners are only Imported for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task ThenTheMatchedLearnersAreOnlyImportedForCollectionPeriodAndAcademicYear(byte collectionPeriod, short academicYear)
        {
            var timer = new Stopwatch();

            timer.Start();

            var dataLockEvents = new List<DataLockEventModel>();

            while (!dataLockEvents.Any() && timer.Elapsed < _testContext.TimeToWait)
            {
                dataLockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(1000);

                if (!dataLockEvents.Any())
                    Thread.Sleep(_testContext.TimeToPause);
            }

            AssertSingleDataLockEventForPeriod(dataLockEvents, collectionPeriod, academicYear);

            timer.Stop();

            await _testContext.TestRepository.ClearDataLockEvent(1000, 2000);
        }

        [Then("the matched Learners are Imported")]
        public async Task ThenTheMatchedLearnersAreImported()
        {
            var timer = new Stopwatch();

            timer.Start();

            var dataLockEvents = new List<DataLockEventModel>();

            while (!dataLockEvents.Any() && timer.Elapsed < _testContext.TimeToWait)
            {
                dataLockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(1000);

                if (!dataLockEvents.Any())
                    Thread.Sleep(_testContext.TimeToPause);
            }

            AssertSingleDataLockEventForPeriod(dataLockEvents, 1, 2021);

            timer.Stop();

            await _testContext.TestRepository.ClearDataLockEvent(1000, 2000);
        }

        public void AssertSingleDataLockEventForPeriod(List<DataLockEventModel> dataLockEvents, byte collectionPeriod, short academicYear)
        {
            dataLockEvents.Count.Should().Be(1);
            var actual = dataLockEvents.First();

            actual.Ukprn.Should().Be(1000);
            actual.ContractType.Should().Be(ContractType.Act1);
            actual.CollectionPeriod.Should().Be(collectionPeriod);
            actual.AcademicYear.Should().Be(academicYear);
            actual.LearnerReferenceNumber.Should().Be("ref#");

            actual.LearnerUln.Should().Be(2000);
            actual.LearningAimReference.Should().Be("ZPROG001");
            actual.LearningAimProgrammeType.Should().Be(100);
            actual.LearningAimStandardCode.Should().Be(200);
            actual.LearningAimFrameworkCode.Should().Be(300);
            actual.LearningAimPathwayCode.Should().Be(400);
            actual.LearningAimFundingLineType.Should().Be("funding");
            actual.IlrSubmissionDateTime.Should().Be(new DateTime(2020, 10, 10));
            actual.IsPayable.Should().Be(false);
            actual.DataLockSource.Should().Be(0);
            actual.JobId.Should().Be(123);
            actual.EventTime.Date.Should().Be(DateTimeOffset.Now.Date);
            actual.LearningStartDate.Should().Be(new DateTime(2020, 10, 09));

            //(DataLockEventId, PriceEpisodeIdentifier, SfaContributionPercentage, TotalNegotiatedPrice1, TotalNegotiatedPrice2, TotalNegotiatedPrice3, TotalNegotiatedPrice4, StartDate, EffectiveTotalNegotiatedPriceStartDate, PlannedEndDate, ActualEndDate, NumberOfInstalments, InstalmentAmount, CompletionAmount, Completed)
            //VALUES (@dataLockEventId2, '25-104-01/08/2020', 1, 1000, 2000, 0, 0, '2020-10-07', '2021-01-01', '2020-10-11', '2020-10-12', 12, 50, 550, 0)

            actual.PriceEpisodes.Count.Should().Be(1);
            var actualPriceEpisodes = actual.PriceEpisodes.First();
            actualPriceEpisodes.DataLockEventId.Should().Be(actual.EventId);
            actualPriceEpisodes.PriceEpisodeIdentifier.Should().Be("25-104-01/08/2020");
            actualPriceEpisodes.SfaContributionPercentage.Should().Be(1);
            actualPriceEpisodes.TotalNegotiatedPrice1.Should().Be(1000);
            actualPriceEpisodes.TotalNegotiatedPrice2.Should().Be(2000);
            actualPriceEpisodes.TotalNegotiatedPrice3.Should().Be(0);
            actualPriceEpisodes.TotalNegotiatedPrice4.Should().Be(0);
            actualPriceEpisodes.StartDate.Should().Be(new DateTime(2020, 10, 07));
            actualPriceEpisodes.EffectiveTotalNegotiatedPriceStartDate.Should().Be(new DateTime(2021, 01, 01));
            actualPriceEpisodes.PlannedEndDate.Should().Be(new DateTime(2020, 10, 11));
            actualPriceEpisodes.ActualEndDate.Should().Be(new DateTime(2020, 10, 12));
            actualPriceEpisodes.NumberOfInstalments.Should().Be(12);
            actualPriceEpisodes.InstalmentAmount.Should().Be(50);
            actualPriceEpisodes.CompletionAmount.Should().Be(550);
            actualPriceEpisodes.Completed.Should().Be(false);

            //INSERT INTO Payments2.DataLockEventPayablePeriod (DataLockEventId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage, LearningStartDate, ApprenticeshipId)
            //VALUES  (@dataLockEventId2, '25-104-01/08/2020', 1, 1, 100, 1, @testDateTime, 123456),
            //        (@dataLockEventId2, '25-104-01/08/2020', 1, 2, 200, 1, @testDateTime, 123456),
            //        (@dataLockEventId2, '25-104-01/08/2020', 1, 3, 300, 1, @testDateTime, 123456)

            actual.PayablePeriods.Count.Should().Be(3);
            actual.PayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 1,
                Amount = 100M,
                SfaContributionPercentage = 1M,
                ApprenticeshipId = 123456L
            });

            actual.PayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 2,
                Amount = 200M,
                SfaContributionPercentage = 1,
                ApprenticeshipId = 123456L
            });

            actual.PayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 3,
                Amount = 300M,
                SfaContributionPercentage = 1,
                ApprenticeshipId = 123456L
            });

            //INSERT INTO Payments2.DataLockEventNonPayablePeriod (DataLockEventId, DataLockEventNonPayablePeriodId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage)
            //VALUES  (@dataLockEventId2, @dataLockEventFailureId1, '25-104-01/08/2020', 1, 3, 400, 1),
            //        (@dataLockEventId2, @dataLockEventFailureId2, '25-104-01/08/2020', 1, 4, 500, 1),
            //        (@dataLockEventId2, @dataLockEventFailureId3, '25-104-01/08/2020', 1, 5, 600, 1),
            //        (@dataLockEventId2, @dataLockEventFailureId4, '25-104-01/08/2020', 1, 6, 600, 1)

            actual.NonPayablePeriods.Count.Should().Be(4);
            actual.NonPayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 3,
                Amount = 400,
                SfaContributionPercentage = 1,
            });

            actual.NonPayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 4,
                Amount = 500,
                SfaContributionPercentage = 1,
            });

            actual.NonPayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 5,
                Amount = 600,
                SfaContributionPercentage = 1,
            });

            actual.NonPayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 6,
                Amount = 600,
                SfaContributionPercentage = 1,
            });

            //INSERT INTO Payments2.DataLockEventNonPayablePeriodFailures (DataLockEventNonPayablePeriodId, DataLockFailureId, ApprenticeshipId)
            //VALUES  (@dataLockEventFailureId1, 1, 123456), 
            //        (@dataLockEventFailureId1, 2, 123456), 
            //        (@dataLockEventFailureId1, 3, 123456), 
            //        (@dataLockEventFailureId2, 7, 123456), 
            //        (@dataLockEventFailureId3, 9, 123456),
            //        (@dataLockEventFailureId4, 1, 12345600)

            var actualNonPayablePeriodFailures = actual.NonPayablePeriods.SelectMany(np => np.Failures).ToList();

            actualNonPayablePeriodFailures.Count.Should().Be(6);

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 1,
                ApprenticeshipId = 123456
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 2,
                ApprenticeshipId = 123456
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 3,
                ApprenticeshipId = 123456
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 7,
                ApprenticeshipId = 123456
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 9,
                ApprenticeshipId = 123456
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 1,
                ApprenticeshipId = 12345600
            });
        }
    }
}

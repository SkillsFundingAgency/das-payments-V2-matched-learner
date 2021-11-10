using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests.Bindings
{
    [Binding]
    public class TestBindingSteps
    {
        private readonly TestContext _testContext;
        private readonly long _ukprn;
        private readonly long _learnerUln;
        private readonly long _apprenticeshipId;
        private readonly TestApplicationSettings _settings;

        public TestBindingSteps(TestContext testContext)
        {
            var random = new Random();

            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);
            _apprenticeshipId = _ukprn + _learnerUln;

            _testContext = testContext;
            _settings = TestConfiguration.TestApplicationSettings;
        }

        [Given("A Submission Job Succeeded for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task GivenASuccessfulSubmissionIsCompletedForPeriod(byte collectionPeriod, short academicYear)
        {
            await _testContext.TestRepository.ClearDataLockEvent(_ukprn, _learnerUln);
            await _testContext.TestRepository.AddDataLockEvent(_ukprn, _learnerUln, collectionPeriod, academicYear, false);
        }

        [Given("there is existing data For CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task GivenExistingDataForPeriod(byte collectionPeriod, short academicYear)
        {
            _testContext.ExistingMatchedLearnerDataLockId = await _testContext.TestRepository.AddDataLockEvent(_ukprn, _learnerUln, collectionPeriod, academicYear, true);
        }

        [When("A SubmissionJobSucceeded message is received for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task WhenWeReceiveSubmissionSucceededEventForPeriod(byte collectionPeriod, short academicYear)
        {
            await _testContext.TestEndpointInstance.PublishSubmissionSucceededEvent(_ukprn, academicYear, collectionPeriod);
        }

        [Then("the matched Learners are only Imported for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task ThenTheMatchedLearnersAreOnlyImportedForCollectionPeriodAndAcademicYear(byte collectionPeriod, short academicYear)
        {
            var timer = new Stopwatch();

            timer.Start();

            var dataLockEvents = new List<DataLockEventModel>();

            while (!dataLockEvents.Any() && timer.Elapsed < _settings.TimeToWait)
            {
                dataLockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(_ukprn);

                if (!dataLockEvents.Any())
                    Thread.Sleep(_settings.TimeToPause);
                else
                    Thread.Sleep(_settings.TimeToWait - timer.Elapsed);
            }

            AssertSingleDataLockEventForPeriod(dataLockEvents, collectionPeriod, academicYear);

            timer.Stop();

            await _testContext.TestRepository.ClearDataLockEvent(_ukprn, _learnerUln);
        }

        [Then("the existing matched Learners are NOT deleted")]
        public async Task ThenTheExistingMatchedLearnersAreNotDeleted()
        {
            var timer = new Stopwatch();

            timer.Start();

            IEnumerable<DataLockEventModel> existingMatchedLearnerDataLockEvents = new List<DataLockEventModel>();
            var first = true;

            while (first || (existingMatchedLearnerDataLockEvents.Any() && timer.Elapsed < _settings.TimeToWaitUnexpected))
            {
                var dataLockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(_ukprn);
                existingMatchedLearnerDataLockEvents = dataLockEvents.Where(x => x.EventId == _testContext.ExistingMatchedLearnerDataLockId).ToList();

                Thread.Sleep(_settings.TimeToPause);
                first = false;
            }

            timer.Stop();

            existingMatchedLearnerDataLockEvents.Should().NotBeEmpty();
        }

        [Then("the existing matched Learners are deleted")]
        public async Task ThenTheExistingMatchedLearnersAreDeleted()
        {
            var timer = new Stopwatch();

            timer.Start();

            IEnumerable<DataLockEventModel> existingMatchedLearnerDataLockEvents = new List<DataLockEventModel>();
            var first = true;

            while (first || (existingMatchedLearnerDataLockEvents.Any() && timer.Elapsed < _settings.TimeToWait))
            {
                var dataLockEvents = await _testContext.TestRepository.GetMatchedLearnerDataLockEvents(_ukprn);
                existingMatchedLearnerDataLockEvents = dataLockEvents.Where(x => x.EventId == _testContext.ExistingMatchedLearnerDataLockId).ToList();

                if (existingMatchedLearnerDataLockEvents.Any())
                    Thread.Sleep(_settings.TimeToPause);
                first = false;
            }

            timer.Stop();

            existingMatchedLearnerDataLockEvents.Should().BeEmpty();
        }

        public void AssertSingleDataLockEventForPeriod(List<DataLockEventModel> dataLockEvents, byte collectionPeriod, short academicYear)
        {
            var dataLockEventsForPeriod = dataLockEvents.Where(x =>
                x.AcademicYear == academicYear && x.CollectionPeriod == collectionPeriod).ToList();

            dataLockEventsForPeriod.Count.Should().Be(1);
            var actual = dataLockEventsForPeriod.First();

            actual.Ukprn.Should().Be(_ukprn);
            actual.ContractType.Should().Be(ContractType.Act1);
            actual.CollectionPeriod.Should().Be(collectionPeriod);
            actual.AcademicYear.Should().Be(academicYear);
            actual.LearnerReferenceNumber.Should().Be("ref#");

            actual.LearnerUln.Should().Be(_learnerUln);
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
            //VALUES (@dataLockEventId2, '25-104-01/08/2020', 1, _ukprn, _learnerUln, 0, 0, '2020-10-07', '2021-01-01', '2020-10-11', '2020-10-12', 12, 50, 550, 0)

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
            //VALUES  (@dataLockEventId2, '25-104-01/08/2020', 1, 1, 100, 1, @testDateTime, _apprenticeshipId),
            //        (@dataLockEventId2, '25-104-01/08/2020', 1, 2, 200, 1, @testDateTime, _apprenticeshipId),
            //        (@dataLockEventId2, '25-104-01/08/2020', 1, 3, 300, 1, @testDateTime, _apprenticeshipId)

            actual.PayablePeriods.Count.Should().Be(3);
            actual.PayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 1,
                Amount = 100M,
                SfaContributionPercentage = 1M,
                ApprenticeshipId = _apprenticeshipId
            });

            actual.PayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 2,
                Amount = 200M,
                SfaContributionPercentage = 1,
                ApprenticeshipId = _apprenticeshipId
            });

            actual.PayablePeriods.Should().ContainEquivalentOf(new
            {
                DataLockEventId = actual.EventId,
                PriceEpisodeIdentifier = "25-104-01/08/2020",
                TransactionType = 1,
                DeliveryPeriod = 3,
                Amount = 300M,
                SfaContributionPercentage = 1,
                ApprenticeshipId = _apprenticeshipId
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
            //VALUES  (@dataLockEventFailureId1, 1, _apprenticeshipId), 
            //        (@dataLockEventFailureId1, 2, _apprenticeshipId), 
            //        (@dataLockEventFailureId1, 3, _apprenticeshipId), 
            //        (@dataLockEventFailureId2, 7, _apprenticeshipId), 
            //        (@dataLockEventFailureId3, 9, _apprenticeshipId),
            //        (@dataLockEventFailureId4, 1, 9876500)

            var actualNonPayablePeriodFailures = actual.NonPayablePeriods.SelectMany(np => np.Failures).ToList();

            actualNonPayablePeriodFailures.Count.Should().Be(6);

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 1,
                ApprenticeshipId = _apprenticeshipId
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 2,
                ApprenticeshipId = _apprenticeshipId
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 3,
                ApprenticeshipId = _apprenticeshipId
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 7,
                ApprenticeshipId = _apprenticeshipId
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 9,
                ApprenticeshipId = _apprenticeshipId
            });

            actualNonPayablePeriodFailures.Should().ContainEquivalentOf(new
            {
                DataLockFailureId = 1,
                ApprenticeshipId = 9876500
            });
        }
    }
}

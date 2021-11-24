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
    [Scope(Feature = "SubmissionDataImportTests")]
    public class SubmissionDataImportTestBindings
    {
        private readonly TestContext _testContext;
        private readonly long _ukprn;
        private readonly long _learnerUln;
        private readonly long _apprenticeshipId;
        private readonly TestApplicationSettings _settings;

        public SubmissionDataImportTestBindings(TestContext testContext)
        {
            var random = new Random();

            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);
            _apprenticeshipId = _ukprn + _learnerUln;

            _testContext = testContext;
            _settings = TestConfiguration.TestApplicationSettings;
        }

        [Given("A Submission Job Succeeded for CollectionPeriod (.*) and AcademicYear (.*)")]
        [When("A Submission Job Succeeded for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task GivenASuccessfulSubmissionIsCompletedForPeriod(byte collectionPeriod, short academicYear)
        {
            await _testContext.TestRepository.ClearMatchedLearnerTrainings(_ukprn, _learnerUln);
            await _testContext.TestRepository.ClearDataLockEvent(_ukprn, _learnerUln);
            await _testContext.TestRepository.AddDataLockEvent(_ukprn, _learnerUln, collectionPeriod, academicYear, false);
        }

        [Given("there is existing Trainings data For CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task GivenExistingTrainingsDataForPeriod(byte collectionPeriod, short academicYear)
        {
            _testContext.ExistingMatchedLearnerTrainingId = await _testContext.TestRepository.AddMatchedLearnerTrainings(_ukprn, _learnerUln, collectionPeriod, academicYear, true);
        }

        [Then("the matched Learners Trainings are only Imported for CollectionPeriod (.*) and AcademicYear (.*)")]
        [Given("the matched Learners Trainings are only Imported for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task ThenTheMatchedLearnersTrainingsAreOnlyImportedForCollectionPeriodAndAcademicYear(byte collectionPeriod, short academicYear)
        {
            var timer = new Stopwatch();

            timer.Start();

            var learnerTrainings = new List<TrainingModel>();

            while (!learnerTrainings.Any() && timer.Elapsed < _settings.TimeToWait)
            {
                learnerTrainings = await _testContext.TestRepository.GetMatchedLearnerTrainings(_ukprn);

                if (!learnerTrainings.Any())
                    Thread.Sleep(_settings.TimeToPause);
                else
                    Thread.Sleep(_settings.TimeToWait - timer.Elapsed);
            }

            var learnerTrainingForCollectionPeriod = learnerTrainings.Where(x =>
                x.AcademicYear == academicYear && x.IlrSubmissionWindowPeriod == collectionPeriod).ToList();

            if (learnerTrainingForCollectionPeriod.Count != 1)
            {
                Console.WriteLine($"Failed to Find matching dataLockEvents For Period {collectionPeriod} : {academicYear}, ukprn {_ukprn}, learnerUln {_learnerUln}");
            }

            learnerTrainingForCollectionPeriod.Count.Should().Be(1);

            AssertSingleLearnerTrainingForCollectionPeriod(learnerTrainingForCollectionPeriod.First(), collectionPeriod, academicYear);

            await _testContext.TestRepository.ClearMatchedLearnerTrainings(_ukprn, _learnerUln);

            timer.Stop();
        }

        [When("A SubmissionJobSucceeded message is received for CollectionPeriod (.*) and AcademicYear (.*)")]
        [Given("A SubmissionJobSucceeded message is received for CollectionPeriod (.*) and AcademicYear (.*)")]
        public async Task WhenWeReceiveSubmissionSucceededEventForPeriod(byte collectionPeriod, short academicYear)
        {
            await _testContext.TestEndpointInstance.PublishSubmissionSucceededEvent(_ukprn, academicYear, collectionPeriod);
        }

        [Then("the existing matched Learners Trainings are NOT deleted")]
        public async Task ThenTheExistingMatchedLearnersTrainingAreNotDeleted()
        {
            var timer = new Stopwatch();

            timer.Start();

            IEnumerable<TrainingModel> existingMatchedLearnerTrainings = new List<TrainingModel>();
            var first = true;

            while (first || (existingMatchedLearnerTrainings.Any() && timer.Elapsed < _settings.TimeToWaitUnexpected))
            {
                var matchedLearnerTrainings = await _testContext.TestRepository.GetMatchedLearnerTrainings(_ukprn);
                existingMatchedLearnerTrainings = matchedLearnerTrainings.Where(x => x.Id == _testContext.ExistingMatchedLearnerTrainingId).ToList();

                Thread.Sleep(_settings.TimeToPause);
                first = false;
            }

            timer.Stop();

            existingMatchedLearnerTrainings.Should().NotBeEmpty();
        }

        [Then("the existing matched Learners Trainings are deleted")]
        public async Task ThenTheExistingMatchedLearnersTrainingAreDeleted()
        {
            var timer = new Stopwatch();

            timer.Start();

            IEnumerable<TrainingModel> existingMatchedLearnerTrainings = new List<TrainingModel>();
            var first = true;

            while (first || (existingMatchedLearnerTrainings.Any() && timer.Elapsed < _settings.TimeToWait))
            {
                var matchedLearnerTrainings = await _testContext.TestRepository.GetMatchedLearnerTrainings(_ukprn);
                existingMatchedLearnerTrainings = matchedLearnerTrainings.Where(x => x.Id == _testContext.ExistingMatchedLearnerTrainingId).ToList();

                if (existingMatchedLearnerTrainings.Any())
                    Thread.Sleep(_settings.TimeToPause);
                first = false;
            }

            timer.Stop();

            existingMatchedLearnerTrainings.Should().BeEmpty();
        }

        public void AssertSingleLearnerTrainingForCollectionPeriod(TrainingModel actual, byte collectionPeriod, short academicYear)
        {
            actual.Ukprn.Should().Be(_ukprn);
            actual.IlrSubmissionWindowPeriod.Should().Be(collectionPeriod);
            actual.AcademicYear.Should().Be(academicYear);

            actual.Uln.Should().Be(_learnerUln);
            actual.Reference.Should().Be("ZPROG001");
            actual.ProgrammeType.Should().Be(100);
            actual.StandardCode.Should().Be(200);
            actual.FrameworkCode.Should().Be(300);
            actual.PathwayCode.Should().Be(400);
            actual.FundingLineType.Should().Be("funding");
            actual.IlrSubmissionDate.Should().Be(new DateTime(2020, 10, 10));

            actual.EventTime.Date.Should().Be(DateTimeOffset.Now.Date);
            actual.StartDate.Should().Be(new DateTime(2020, 10, 09));

            actual.PriceEpisodes.Count.Should().Be(1);
            var actualPriceEpisodes = actual.PriceEpisodes.First();
            actualPriceEpisodes.TrainingId.Should().Be(actual.Id);
            actualPriceEpisodes.Identifier.Should().Be("25-104-01/08/2020");
            actualPriceEpisodes.AgreedPrice.Should().Be(3000);
            actualPriceEpisodes.StartDate.Should().Be(new DateTime(2020, 08, 01));
            actualPriceEpisodes.TotalNegotiatedPriceStartDate.Should().Be(new DateTime(2021, 01, 01));
            actualPriceEpisodes.PlannedEndDate.Should().Be(new DateTime(2020, 10, 11));
            actualPriceEpisodes.ActualEndDate.Should().Be(new DateTime(2020, 10, 12));
            actualPriceEpisodes.NumberOfInstalments.Should().Be(12);
            actualPriceEpisodes.InstalmentAmount.Should().Be(50);
            actualPriceEpisodes.CompletionAmount.Should().Be(550);

            actualPriceEpisodes.Periods.Count.Should().Be(7);
            
            actualPriceEpisodes.Periods.Should().ContainEquivalentOf(new
            {
                Period = 1,
                Amount = 100,
                IsPayable = true,
                TransactionType = 1,

                ApprenticeshipId = _apprenticeshipId,
                AccountId = 1000,
                TransferSenderAccountId = 500,
                ApprenticeshipEmployerType = 3,
            });

            actualPriceEpisodes.Periods.Should().ContainEquivalentOf(new
            {
                Period = 2,
                Amount = 200,
                IsPayable = true,
                TransactionType = 1,

                ApprenticeshipId = _apprenticeshipId,
                AccountId = 1000,
                TransferSenderAccountId = 500,
                ApprenticeshipEmployerType = 3,
            });

            actualPriceEpisodes.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                Amount = 300,
                IsPayable = true,
                TransactionType = 1,

                ApprenticeshipId = _apprenticeshipId,
                AccountId = 1000,
                TransferSenderAccountId = 500,
                ApprenticeshipEmployerType = 3,
            });

            actualPriceEpisodes.Periods.Should().ContainEquivalentOf(new
            {
                Period = 3,
                Amount = 400,
                IsPayable = false,
                TransactionType = 1,

                ApprenticeshipId = _apprenticeshipId,
                AccountId = 1000,
                TransferSenderAccountId = 500,
                ApprenticeshipEmployerType = 3,

                FailedDataLock1 = true,
                FailedDataLock2 = true,
                FailedDataLock3 = true,
            });

            actualPriceEpisodes.Periods.Should().ContainEquivalentOf(new
            {
                Period = 4,
                Amount = 500,
                IsPayable = false,
                TransactionType = 1,

                ApprenticeshipId = _apprenticeshipId,
                AccountId = 1000,
                TransferSenderAccountId = 500,
                ApprenticeshipEmployerType = 3,

                FailedDataLock7 = true,
            });

            actualPriceEpisodes.Periods.Should().ContainEquivalentOf(new
            {
                Period = 5,
                Amount = 600,
                IsPayable = false,
                TransactionType = 1,

                ApprenticeshipId = _apprenticeshipId,
                AccountId = 1000,
                TransferSenderAccountId = 500,
                ApprenticeshipEmployerType = 3,

                FailedDataLock9 = true,
            });

            actualPriceEpisodes.Periods.Should().ContainEquivalentOf(new
            {
                Period = 6,
                Amount = 600,
                IsPayable = false,
                TransactionType = 1,

                //ApprenticeshipId = null,
                AccountId = 0,
                TransferSenderAccountId = 0,
                ApprenticeshipEmployerType = 0,

                FailedDataLock1 = true,
            });
        }
    }
}
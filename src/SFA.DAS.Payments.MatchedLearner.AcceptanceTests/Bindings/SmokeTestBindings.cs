using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
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
        private readonly List<TrainingModel> _expectedTrainings = new List<TrainingModel>();
        private bool _useV1Api;
        private bool _singleTrainingMultiYear;
        public SmokeTestBindings(SmokeTestContext context)
        {
            _context = context;

            var random = new Random();

            _ukprn = random.Next(100000);
            _learnerUln = random.Next(100000);
            _apprenticeshipId = _ukprn + _learnerUln;
        }

        [Given("we have created (.*) sample learners in Legacy Schema")]
        public async Task GivenWeHaveCreatedASampleLearner(int learnerCount)
        {
            _useV1Api = true;
            var repository = new TestRepository();
            var ukprn = _ukprn;
            var learnerUln = _learnerUln;
            for (var index = 1; index < learnerCount + 1; index++)
            {
                
                await repository.ClearLearner(ukprn, learnerUln);
                await repository.AddDataLockEvent(ukprn, learnerUln);

                ukprn += index;
                learnerUln += index;
            }
        }

        [When("we call the API (.*) times with the sample learners details in Legacy Schema")]
        public void WhenWeCallTheApiTimesWithTheSampleLearnersDetails(int learnerCount)
        {
            _useV1Api = true;
            var request = new TestClient(_useV1Api);
            var ukprn = _ukprn;
            var learnerUln = _learnerUln;

            for (var index = 1; index < learnerCount + 1; index++)
            {
                var currentUkprn = ukprn;
                var currentUln = learnerUln;
                _context.Requests.Add(request.Awaiting(client => client.Handle(currentUkprn, currentUln)));
                ukprn += index;
                learnerUln += index;
            }
        }

        [When("we call the API with a learner that does not exist in Legacy Schema")]
        public void WhenWeCallTheApiWithALearnerThatDoesNotExist()
        {
            _useV1Api = true;
            var request = new TestClient(_useV1Api);
            var act = request.Awaiting(client => client.Handle(0, 0));
            _context.FailedRequest = act;
        }

        [When("we call the API with the sample learners details in Legacy Schema")]
        public async Task WhenWeCallTheApiWithTheSampleLearnersDetails()
        {
            _useV1Api = true;

            var request = new TestClient(_useV1Api);

            _context.MatchedLearnerDto = await request.Handle(_ukprn, _learnerUln);
        }

        [Then("the result should be a (.*)")]
        public void ThenTheResultShouldBeA(int p0)
        {
            _context.FailedRequest.Should().ThrowAsync<Exception>().WithMessage($"{p0}");
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
            actual.IlrSubmissionDate.Date.Should().Be(new DateTime(2020, 10, 10));
            actual.IlrSubmissionWindowPeriod.Should().Be(1);
            actual.AcademicYear.Should().Be(2021);
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

            //TODO: Fix this
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


            //TODO: Fix this
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
                DataLockFailures = new HashSet<byte> { 1, 2, 3 },
            });
            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 4,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte> { 7 },
            });
            priceEpisode2.Periods.Should().ContainEquivalentOf(new
            {
                Period = 5,
                IsPayable = false,
                AccountId = 1000,
                ApprenticeshipId = _apprenticeshipId,
                ApprenticeshipEmployerType = 3,
                TransferSenderAccountId = 500,
                DataLockFailures = new HashSet<byte> { 9 },
            });
        }






        [Given("we have created a sample learner with (.*) Training Records with (.*) Price Episode across (.*) academic Year")]
        public async Task GivenWeHaveCreatedASampleLearnerTraining(int numberOfTraining, int numberOfPriceEpisode, int numberOfAcademicYear)
        {
            var repository = new TestRepository();
            await repository.ClearMatchedLearnerTrainings(_ukprn, _learnerUln);

            _singleTrainingMultiYear = numberOfAcademicYear > 1 && numberOfTraining == 1;
            var singlePriceEpisodeMultiYear = numberOfPriceEpisode == 1 && _singleTrainingMultiYear;

            var course = 100;
            var agreedPrice = 3000;
            short academicYear = 2122;
            byte collectionPeriod = 1;

            for (var i = 0; i < numberOfAcademicYear; i++)
            {
                course = _singleTrainingMultiYear ? course : course + 1;
                var priceEpisodes = new List<PriceEpisodeModel>();
                for (var j = 0; j < numberOfPriceEpisode; j++)
                {
                    agreedPrice = singlePriceEpisodeMultiYear ? agreedPrice : agreedPrice+1;
                    priceEpisodes.Add(repository.CreatePriceEpisodes(agreedPrice, collectionPeriod, academicYear, _apprenticeshipId));
                    collectionPeriod++;
                }

                var training = await repository.AddMatchedLearnerTrainings(course, _ukprn, _learnerUln, 1, academicYear, priceEpisodes);

                academicYear = (short)(academicYear + 101);

                _expectedTrainings.Add(training);
            }
        }

        [When("we call the V2 API with the sample learners details")]
        public async Task WhenWeCallTheV2ApiWithTheSampleLearnersDetails()
        {
            var request = new TestClient(_useV1Api);

            _context.MatchedLearnerDto = await request.Handle(_ukprn, _learnerUln);
        }

        [Then("the result should contain (.*) Training with (.*) price episode and (.*) Periods")]
        public void ThenTheResultMatchesTheSampleLearnerTraining(int numberOfTraining, int numberOfPriceEpisode, int numberOfPeriod)
        {
            var expectedTrainings = _expectedTrainings
                .OrderByDescending(t => t.AcademicYear)
                .ThenByDescending(t => t.IlrSubmissionWindowPeriod)
                .ToList();

            var expectedHeader = expectedTrainings.First();

            var actual = _context.MatchedLearnerDto;

            actual.Should().NotBeNull();

            actual.StartDate.Date.Should().Be(expectedHeader.StartDate);
            actual.IlrSubmissionDate.Date.Should().Be(expectedHeader.IlrSubmissionDate);
            actual.IlrSubmissionWindowPeriod.Should().Be(expectedHeader.IlrSubmissionWindowPeriod);
            actual.AcademicYear.Should().Be(expectedHeader.AcademicYear);
            actual.Ukprn.Should().Be(_ukprn);
            actual.Uln.Should().Be(_learnerUln);
            actual.Training.Should().HaveCount(numberOfTraining);

            for (var i = 0; i < numberOfTraining; i++)
            {
                var expectedTraining = expectedTrainings.ElementAt(i);
                var actualTraining = actual.Training.ElementAt(i);

                actualTraining.StartDate.Should().Be(expectedTraining.StartDate);
                actualTraining.FrameworkCode.Should().Be(expectedTraining.FrameworkCode);
                actualTraining.FundingLineType.Should().BeNullOrEmpty();
                actualTraining.PathwayCode.Should().Be(expectedTraining.PathwayCode);
                actualTraining.ProgrammeType.Should().Be(expectedTraining.ProgrammeType);
                actualTraining.Reference.Should().Be(expectedTraining.Reference);
                actualTraining.StandardCode.Should().Be(expectedTraining.StandardCode);
                actualTraining.PriceEpisodes.Should().HaveCount(numberOfPriceEpisode);

                List<PriceEpisodeModel> expectedPriceEpisodes;

                if (_singleTrainingMultiYear)
                {
                    //TestLearnerSingleTrainingAcrossMultipleAcademicYearWithMultiplePriceEpisodeForEachTraining
                    //in the single training scenario we expecting all price episodes from all trainings to be returned
                    // this leaves us expecting price episodes from all trainings? is that right?
                    expectedPriceEpisodes = expectedTrainings
                        .SelectMany(t => t.PriceEpisodes)
                        .OrderByDescending(t => t.AcademicYear)
                        .ThenByDescending(t => t.CollectionPeriod)
                        .ToList();
                }
                else
                {
                    ////TestLearnerMultipleTrainingAcrossMultipleAcademicYear
                    ////in the scenario where there are multiple different trainings we only want the price episodes for the current training
                    expectedPriceEpisodes = expectedTraining
                        .PriceEpisodes
                        .OrderByDescending(t => t.AcademicYear)
                        .ThenByDescending(t => t.CollectionPeriod)
                        .ToList(); // if we do this instead should be just the current expected training we are dealing with
                }

                for (var j = 0; j < numberOfPriceEpisode; j++)
                {
                    var actualPriceEpisode = actualTraining.PriceEpisodes.ElementAt(j);
                    var expectedPriceEpisode = expectedPriceEpisodes.ElementAt(j);

                    actualPriceEpisode.Identifier.Should().Be(expectedPriceEpisode.Identifier);
                    actualPriceEpisode.AcademicYear.Should().Be(expectedPriceEpisode.AcademicYear);
                    actualPriceEpisode.CollectionPeriod.Should().Be(expectedPriceEpisode.CollectionPeriod);
                    actualPriceEpisode.AgreedPrice.Should().Be(expectedPriceEpisode.AgreedPrice);
                    actualPriceEpisode.StartDate.Should().Be(expectedPriceEpisode.StartDate);
                    actualPriceEpisode.EndDate.Should().Be(expectedPriceEpisode.ActualEndDate);
                    actualPriceEpisode.NumberOfInstalments.Should().Be(expectedPriceEpisode.NumberOfInstalments);
                    actualPriceEpisode.InstalmentAmount.Should().Be(expectedPriceEpisode.InstalmentAmount);
                    actualPriceEpisode.CompletionAmount.Should().Be(expectedPriceEpisode.CompletionAmount);
                    actualPriceEpisode.TotalNegotiatedPriceStartDate.Should().Be(expectedPriceEpisode.TotalNegotiatedPriceStartDate);
                    actualPriceEpisode.Periods.Should().HaveCount(numberOfPeriod);

                    var expectedPeriods = expectedTrainings.SelectMany(t => t.PriceEpisodes).SelectMany(p => p.Periods).ToList();

                    for (var k = 0; k < numberOfPeriod; k++)
                    {
                        var actualPeriod = actualPriceEpisode.Periods.ElementAt(k);
                        var expectedPeriod = expectedPeriods.First(pd => pd.AccountId == actualPeriod.AccountId);

                        actualPeriod.Period.Should().Be(expectedPeriod.Period);
                        actualPeriod.IsPayable.Should().Be(expectedPeriod.IsPayable);
                        actualPeriod.AccountId.Should().Be(expectedPeriod.AccountId);
                        actualPeriod.ApprenticeshipId.Should().Be(expectedPeriod.ApprenticeshipId);
                        actualPeriod.ApprenticeshipEmployerType.Should().Be(expectedPeriod.ApprenticeshipEmployerType);
                        actualPeriod.TransferSenderAccountId.Should().Be(expectedPeriod.TransferSenderAccountId);

                        actualPeriod.DataLockFailures.Should().BeEmpty();
                    }
                }
            }
        }
    }
}

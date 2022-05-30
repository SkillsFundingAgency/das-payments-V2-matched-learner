using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.MappersTests.MatchedLearnerDtoMapperTests
{
    [TestFixture]
    public class MapTestsWithMultipleDataLockEvents
    {
        private List<DataLockEventModel> _testInputDataLockEvents = new List<DataLockEventModel>();
        private List<ApprenticeshipModel> _testInputApprenticeships = new List<ApprenticeshipModel>();

        [SetUp]
        public void Setup()
        {
            _testInputApprenticeships = new List<ApprenticeshipModel>
            {
                new ApprenticeshipModel
                {
                    Id = 123,
                    AccountId = 456,
                    TransferSendingEmployerAccountId = 789,
                    ApprenticeshipEmployerType = 1,
                }
            };

            var event1 = Guid.NewGuid();
            var nonPayableEventId1 = Guid.NewGuid();

            //TODO: Fix this
            //_testInputDataLockEvents.LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
            //{
            //    new LatestSuccessfulJobModel
            //    {
            //        CollectionPeriod = 14,
            //        AcademicYear = 1920,
            //        IlrSubmissionTime = DateTime.Now,
            //        Ukprn = 1234,
            //        JobId = 1,
            //        DcJobId = 1,
            //    }
            //};

            _testInputDataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 1920,
                    CollectionPeriod = 14,
                    LearningAimPathwayCode = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    EventId = event1,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>
                    {
                        new DataLockEventPriceEpisodeModel
                        {
                            DataLockEventId = event1,
                            TotalNegotiatedPrice1 = 100m,
                            TotalNegotiatedPrice2 = 200m,
                            InstalmentAmount = 2m,
                            NumberOfInstalments = 5,
                            CompletionAmount = 1m,
                            PriceEpisodeIdentifier = "1-1-01/08/2020",
                        },
                        new DataLockEventPriceEpisodeModel
                        {
                            DataLockEventId = event1,
                            TotalNegotiatedPrice1 = 1000m,
                            TotalNegotiatedPrice2 = 2000m,
                            InstalmentAmount = 20m,
                            NumberOfInstalments = 50,
                            CompletionAmount = 10m,
                            PriceEpisodeIdentifier = "2-2-01/08/2020",
                        }
                    },
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                    {
                        new DataLockEventNonPayablePeriodModel
                        {
                            DataLockEventId = event1,
                            DataLockEventNonPayablePeriodId = nonPayableEventId1,
                            PriceEpisodeIdentifier = "1-1-01/08/2020",
                            DeliveryPeriod = 2,
                            TransactionType = 1,
                            Amount = 100,
                            Failures = new List<DataLockEventNonPayablePeriodFailureModel>
                            {
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    DataLockEventNonPayablePeriodId = nonPayableEventId1,
                                    ApprenticeshipId = 123,
                                    DataLockFailureId = 2,
                                },
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    DataLockEventNonPayablePeriodId = nonPayableEventId1,
                                    ApprenticeshipId = 123,
                                    DataLockFailureId = 3,
                                },
                            }
                        }
                    },
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>
                    {
                        new DataLockEventPayablePeriodModel
                        {
                            DataLockEventId = event1,
                            PriceEpisodeIdentifier = "2-2-01/08/2020",
                            DeliveryPeriod = 1,
                            TransactionType = 1,
                            Amount = 100,
                        },
                    }
                }
            };


            //TODO: Fix this
            //_testInputDataLockEvents.LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
            //{
            //    new LatestSuccessfulJobModel
            //    {
            //        CollectionPeriod = 1,
            //        AcademicYear = 2021,
            //        IlrSubmissionTime = DateTime.Now,
            //        Ukprn = 1234,
            //        JobId = 2,
            //        DcJobId = 2,
            //    }
            //};

            var event2 = Guid.NewGuid();
            var nonPayableEventId2 = Guid.NewGuid();

            var dataLockEvent2 = new DataLockEventModel
            {
                AcademicYear = 2021,
                CollectionPeriod = 1,
                LearningAimPathwayCode = 2,
                Ukprn = 1234,
                LearnerUln = 1234,
                EventId = event2,
                LearningAimReference = "ZPROG001",
                PriceEpisodes = new List<DataLockEventPriceEpisodeModel>
                {
                    new DataLockEventPriceEpisodeModel
                    {
                        DataLockEventId = event2,
                        TotalNegotiatedPrice1 = 10000m,
                        TotalNegotiatedPrice2 = 20000m,
                        InstalmentAmount = 200m,
                        NumberOfInstalments = 500,
                        CompletionAmount = 100m,
                        PriceEpisodeIdentifier = "3-3-01/08/2020",
                    },
                    new DataLockEventPriceEpisodeModel
                    {
                        DataLockEventId = event2,
                        TotalNegotiatedPrice1 = 10000m,
                        TotalNegotiatedPrice2 = 20000m,
                        InstalmentAmount = 200m,
                        NumberOfInstalments = 500,
                        CompletionAmount = 100m,
                        PriceEpisodeIdentifier = "4-4-01/08/2020",
                    }
                },
                NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                {
                    new DataLockEventNonPayablePeriodModel
                    {
                        DataLockEventId = event2,
                        PriceEpisodeIdentifier = "3-3-01/08/2020",
                        DeliveryPeriod = 200,
                        TransactionType = 1,
                        Amount = 100,
                        DataLockEventNonPayablePeriodId = nonPayableEventId2,
                        Failures = new List<DataLockEventNonPayablePeriodFailureModel>
                        {
                            new DataLockEventNonPayablePeriodFailureModel
                            {
                                DataLockEventNonPayablePeriodId = nonPayableEventId2,
                                ApprenticeshipId = 1230,
                                DataLockFailureId = 200,
                            },
                            new DataLockEventNonPayablePeriodFailureModel
                            {
                                DataLockEventNonPayablePeriodId = nonPayableEventId2,
                                ApprenticeshipId = 1230,
                                DataLockFailureId = 250,
                            }
                        }
                    }
                },
                PayablePeriods = new List<DataLockEventPayablePeriodModel>
                {
                    new DataLockEventPayablePeriodModel
                    {
                        DataLockEventId = event2,
                        PriceEpisodeIdentifier = "4-4-01/08/2020",
                        DeliveryPeriod = 100,
                        TransactionType = 1,
                        Amount = 100,
                    }
                }
            };

            _testInputDataLockEvents.Add(dataLockEvent2);
        }

        [Test]
        public void InputWithPayablePeriod_Should_ProducePeriodWithIsPayableTrue()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(_testInputDataLockEvents, _testInputApprenticeships);
            actual
                .SelectMany(x => x.PriceEpisodes)
                .SelectMany(x => x.Periods)
                .Should().ContainEquivalentOf(new { IsPayable = true });
        }

        [Test]
        public void Training_Should_HaveTwoElements()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(_testInputDataLockEvents, _testInputApprenticeships);
            actual.Should().HaveCount(2);
        }

        [Test]
        public void Training_Should_HaveExpectedPriceEpisodes()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(_testInputDataLockEvents, _testInputApprenticeships);
            actual.First().PriceEpisodes.Should().HaveCount(2);
            actual.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "1-1-01/08/2020", AcademicYear = 1920, CollectionPeriod = 14 });
            actual.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "2-2-01/08/2020", AcademicYear = 1920, CollectionPeriod = 14 });

            actual.Last().PriceEpisodes.Should().HaveCount(2);
            actual.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "3-3-01/08/2020", AcademicYear = 2021, CollectionPeriod = 1 });
            actual.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "4-4-01/08/2020", AcademicYear = 2021, CollectionPeriod = 1 });
        }

        [Test]
        public void Training_Should_BeInOutput()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(_testInputDataLockEvents, _testInputApprenticeships);

            actual.Should().ContainEquivalentOf(new { PathwayCode = 1 });
            actual.Should().ContainEquivalentOf(new { PathwayCode = 2 });
        }

        [Test]
        public void ThereShouldBe_NoMixingOfPeriodsBetweenPriceEpisodes()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(_testInputDataLockEvents, _testInputApprenticeships);

            var firstEvent = actual.FirstOrDefault(x => x.PathwayCode == 1);
            firstEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(2);
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 2 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 1 });

            var secondEvent = actual.FirstOrDefault(x => x.PathwayCode == 2);
            secondEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(2);
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 200 });
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 100 });
        }
    }
}
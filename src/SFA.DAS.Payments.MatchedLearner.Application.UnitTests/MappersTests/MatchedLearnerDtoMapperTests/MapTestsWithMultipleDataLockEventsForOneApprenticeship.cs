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
    public class MapTestsWithMultipleDataLockEventsForOneApprenticeship
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
            var nonPayableEventId = Guid.NewGuid();


            _testInputDataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    EventId = event1,
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
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
                            PriceEpisodeIdentifier = "1-1-01/08/2020",
                            DeliveryPeriod = 2,
                            DataLockEventNonPayablePeriodId = nonPayableEventId,
                            TransactionType = 1,
                            Amount = 100,
                            Failures = new List<DataLockEventNonPayablePeriodFailureModel>
                            {
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    DataLockEventNonPayablePeriodId = nonPayableEventId,
                                    ApprenticeshipId = 123,
                                    DataLockFailureId = 2,
                                },
                                new DataLockEventNonPayablePeriodFailureModel
                                {
                                    DataLockEventNonPayablePeriodId = nonPayableEventId,
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

            var event2 = Guid.NewGuid();
            var nonPayableEventId2 = Guid.NewGuid();

            _testInputDataLockEvents.Add(new DataLockEventModel
            {
                EventId = event2,
                AcademicYear = 2021,
                CollectionPeriod = 1,
                Ukprn = 1234,
                LearnerUln = 1234,
                LearningAimPathwayCode = 5,
                LearningAimStandardCode = 6,
                LearningAimFrameworkCode = 7,
                LearningAimProgrammeType = 8,
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
                            },
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
                    },
                }
            });
        }

        [Test]
        public void Training_Should_HaveTwoElement()
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
            actual.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "1-1-01/08/2020" });
            actual.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "2-2-01/08/2020" });
            actual.Last().PriceEpisodes.Should().HaveCount(2);
            actual.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "3-3-01/08/2020" });
            actual.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "4-4-01/08/2020" });
        }

        [Test]
        public void Training_Should_BeInOutput()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(_testInputDataLockEvents, _testInputApprenticeships);

            actual.Should().ContainEquivalentOf(new { PathwayCode = 1 });
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
        }

        [Test]
        public void EventsFromSameApprenticeships_Should_BeGrouped()
        {

            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(1);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Uln__Should_NotBeGrouped()
        {
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2122,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2122,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 456,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(2);
        }
        [Test]
        public void EventsFromDifferentApprenticeships_Ukprn__Should_NotBeGrouped()
        {
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2122,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2122,
                    CollectionPeriod = 1,
                    Ukprn = 5678,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Reference__Should_NotBeGrouped()
        {
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 4,
                    LearningAimStandardCode = 3,
                    LearningAimFrameworkCode = 2,
                    LearningAimProgrammeType = 1,
                    LearningAimReference = "1234",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(1);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Pathway__Should_NotBeGrouped()
        {
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 11,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 11,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };


            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Standard__Should_NotBeGrouped()
        {
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 21,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 21,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Framework__Should_NotBeGrouped()
        {
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 31,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 31,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_ProgrammeType__Should_NotBeGrouped()
        {
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 31,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 31,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                },
                new DataLockEventModel
                {
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>());

            actual.Should().HaveCount(2);
        }

        [Test]
        public void WhenPayablePeriodApprenticeshipIsNull_ThenExceptionNotThrown()
        {
            //Arrange
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    EventId = Guid.NewGuid(),
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 31,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>
                    {
                        new DataLockEventPayablePeriodModel
                        {
                            Id = 1,
                            Amount = 100,
                            ApprenticeshipId = null,
                            DataLockEventId = Guid.NewGuid(),
                            DeliveryPeriod = 8,
                            PriceEpisodeIdentifier = "3-490-1-01/08/2020",
                            TransactionType = 2
                        }
                    },
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>(),
                }
            };

            var apprenticeships = new List<ApprenticeshipModel>
            {
                new ApprenticeshipModel
                {
                    Id = 123,
                    AccountId = 456,
                    TransferSendingEmployerAccountId = 789,
                    ApprenticeshipEmployerType = 1,
                }
            };

            //Act
            var sut = new MatchedLearnerDtoMapper();

            //Assert
            Assert.DoesNotThrow(() => { sut.MapToModel(dataLockEvents, apprenticeships); });
        }

        [Test]
        public void WhenNonPayablePeriodApprenticeshipIsNull_ThenExceptionNotThrown()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            var dataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    EventId = eventId,
                    AcademicYear = 2021,
                    CollectionPeriod = 1,
                    Ukprn = 1234,
                    LearnerUln = 1234,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 31,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "ZPROG001",
                    NonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                    {
                        new DataLockEventNonPayablePeriodModel
                        {
                            Id = 1,
                            Amount = 100,
                            DataLockEventId = eventId,
                            DeliveryPeriod = 8,
                            PriceEpisodeIdentifier = "3-490-1-01/08/2020",
                            TransactionType = 2,
                            Failures = null,
                        }
                    },
                    PriceEpisodes = new List<DataLockEventPriceEpisodeModel>(),
                    PayablePeriods = new List<DataLockEventPayablePeriodModel>(),
                }
            };

            //Act
            var sut = new MatchedLearnerDtoMapper();

            //Assert
            Assert.DoesNotThrow(() => { sut.MapToModel(dataLockEvents, new List<ApprenticeshipModel>()); });
        }
    }
}
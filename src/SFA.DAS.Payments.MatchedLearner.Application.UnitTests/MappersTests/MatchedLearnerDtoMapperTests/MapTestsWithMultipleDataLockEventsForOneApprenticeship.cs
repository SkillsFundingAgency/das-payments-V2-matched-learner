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
        private readonly MatchedLearnerDataLockInfo _testInput = new MatchedLearnerDataLockInfo();

        [SetUp]
        public void Setup()
        {
            var event1 = Guid.NewGuid();

            var event2 = Guid.NewGuid();

            //TODO: Fix this
            //_testInput.LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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

            _testInput.DataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    EventId = event1,
                    AcademicYear = 2021,
                    LearningAimPathwayCode = 1,
                    LearningAimStandardCode = 2,
                    LearningAimFrameworkCode = 3,
                    LearningAimProgrammeType = 4,
                    LearningAimReference = "123",
                }
            };

            _testInput.DataLockEventPriceEpisodes = new List<DataLockEventPriceEpisodeModel>
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
            };

            var nonPayableEventId = Guid.NewGuid();

            _testInput.DataLockEventNonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
            {
                new DataLockEventNonPayablePeriodModel
                {
                    DataLockEventId = event1,
                    PriceEpisodeIdentifier = "1-1-01/08/2020",
                    DeliveryPeriod = 2,
                    DataLockEventNonPayablePeriodId = nonPayableEventId
                }
            };

            _testInput.DataLockEventNonPayablePeriodFailures = new List<DataLockEventNonPayablePeriodFailureModel>
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
            };
            _testInput.DataLockEventPayablePeriods = new List<DataLockEventPayablePeriodModel>
            {
                new DataLockEventPayablePeriodModel
                {
                    DataLockEventId = event1,
                    PriceEpisodeIdentifier = "2-2-01/08/2020",
                    DeliveryPeriod = 1,
                },
            };

            _testInput.DataLockEvents.Add(new DataLockEventModel
            {
                EventId = event2,
                AcademicYear = 2021,
                LearningAimPathwayCode = 5,
                LearningAimStandardCode = 6,
                LearningAimFrameworkCode = 7,
                LearningAimProgrammeType = 8,
                LearningAimReference = "123",
            });

            _testInput.DataLockEventPriceEpisodes.AddRange(new List<DataLockEventPriceEpisodeModel>
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
            });

            var nonPayableEventId2 = Guid.NewGuid();

            _testInput.DataLockEventNonPayablePeriods.AddRange(new List<DataLockEventNonPayablePeriodModel>
            {
                new DataLockEventNonPayablePeriodModel
                {
                    DataLockEventId = event2,
                    PriceEpisodeIdentifier = "3-3-01/08/2020",
                    DeliveryPeriod = 200,
                    DataLockEventNonPayablePeriodId = nonPayableEventId2
                }
            });

            _testInput.DataLockEventNonPayablePeriodFailures.AddRange(new List<DataLockEventNonPayablePeriodFailureModel>
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
            });

            _testInput.DataLockEventPayablePeriods.AddRange(new List<DataLockEventPayablePeriodModel>
            {
                new DataLockEventPayablePeriodModel
                {
                    DataLockEventId = event2,
                    PriceEpisodeIdentifier = "4-4-01/08/2020",
                    DeliveryPeriod = 100,
                },
            });
        }

        [Test]
        public void Training_Should_HaveTwoElement()
        {
            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);
            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void Training_Should_HaveExpectedPriceEpisodes()
        {
            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);
            actual.Training.First().PriceEpisodes.Should().HaveCount(2);
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "1-1-01/08/2020" });
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "2-2-01/08/2020" });
            actual.Training.Last().PriceEpisodes.Should().HaveCount(2);
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "3-3-01/08/2020" });
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "4-4-01/08/2020" });
        }

        [Test]
        public void Training_Should_BeInOutput()
        {
            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);

            actual.Training.Should().ContainEquivalentOf(new { PathwayCode = 1 });
        }

        [Test]
        public void ThereShouldBe_NoMixingOfPeriodsBetweenPriceEpisodes()
        {
            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);

            var firstEvent = actual.Training.FirstOrDefault(x => x.PathwayCode == 1);
            firstEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(2);
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 2 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 1 });
        }

        [Test]
        public void EventsFromSameApprenticeships_Should_BeGrouped()
        {
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},
                
                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    },
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                }
            };

            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(1);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Reference__Should_NotBeGrouped()
        {
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},
                
                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "1234",
                    },
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                }
            };

            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Pathway__Should_NotBeGrouped()
        {
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},
                
                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 11,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    },
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                }
            };

            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Standard__Should_NotBeGrouped()
        {
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},
                
                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 21,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    },
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                }
            };

            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Framework__Should_NotBeGrouped()
        {
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},

                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 31,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    },
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                }
            };

            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Programme__Should_NotBeGrouped()
        {
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},

                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 31,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    },
                    new DataLockEventModel
                    {
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 3,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                }
            };

            var sut = new LegacyMatchedLearnerDtoMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void WhenPayablePeriodApprenticeshipIsNull_ThenExceptionNotThrown()
        {
            //Arrange
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},

                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        EventId = Guid.NewGuid(),
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 31,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                },
                DataLockEventPayablePeriods = new List<DataLockEventPayablePeriodModel>
                {
                    new DataLockEventPayablePeriodModel
                    {
                        Id = 1,
                        Amount = 0,
                        ApprenticeshipId = 123,
                        DataLockEventId = Guid.NewGuid(),
                        DeliveryPeriod = 8,
                        PriceEpisodeIdentifier = "3-490-1-01/08/2020",
                        TransactionType = 2
                    }
                }
            };

            //Act
            var sut = new LegacyMatchedLearnerDtoMapper();

            //Assert
            Assert.DoesNotThrow(() => { sut.Map(testInput); });
        }

        [Test]
        public void WhenNonPayablePeriodApprenticeshipIsNull_ThenExceptionNotThrown()
        {
            //Arrange

            var eventId = Guid.NewGuid();

            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
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
                //},

                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
                    {
                        EventId = eventId,
                        AcademicYear = 2021,
                        LearningAimPathwayCode = 1,
                        LearningAimStandardCode = 2,
                        LearningAimFrameworkCode = 31,
                        LearningAimProgrammeType = 4,
                        LearningAimReference = "123",
                    }
                },
                DataLockEventNonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                {
                    new DataLockEventNonPayablePeriodModel
                    {
                        Id = 1,
                        Amount = 0,
                        DataLockEventId = eventId,
                        DeliveryPeriod = 8,
                        PriceEpisodeIdentifier = "3-490-1-01/08/2020",
                        TransactionType = 2
                    }
                }
            };

            //Act
            var sut = new LegacyMatchedLearnerDtoMapper();

            //Assert
            Assert.DoesNotThrow(() => { sut.Map(testInput); });
        }
    }
}
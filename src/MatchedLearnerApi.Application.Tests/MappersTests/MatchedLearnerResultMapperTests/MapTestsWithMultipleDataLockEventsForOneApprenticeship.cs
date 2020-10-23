﻿using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Application.Models;
using NUnit.Framework;

namespace MatchedLearnerApi.Application.Tests.MappersTests.MatchedLearnerResultMapperTests
{
    [TestFixture]
    public class MapTestsWithMultipleDataLockEventsForOneApprenticeship
    {
        private readonly List<DatalockEvent> _testInput = new List<DatalockEvent>();

        [SetUp]
        public void Setup()
        {
            _testInput.Clear();
            _testInput.Add(new DatalockEvent
            {
                AcademicYear = 2021,
                PathwayCode = 1,
                StandardCode = 2,
                FrameworkCode = 3,
                ProgrammeType = 4,
                LearningAimReference = "123",
                PriceEpisodes = new List<DatalockEventPriceEpisode>
                {
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 100m,
                        TotalNegotiatedPrice2 = 200m,
                        InstalmentAmount = 2m,
                        NumberOfInstalments = 5,
                        CompletionAmount = 1m,
                        PriceEpisodeIdentifier = "1-1-1",
                    },
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 1000m,
                        TotalNegotiatedPrice2 = 2000m,
                        InstalmentAmount = 20m,
                        NumberOfInstalments = 50,
                        CompletionAmount = 10m,
                        PriceEpisodeIdentifier = "2-2-2",
                    }
                },
                NonPayablePeriods = new List<DatalockEventNonPayablePeriod>
                {
                    new DatalockEventNonPayablePeriod
                    {
                        DeliveryPeriod = 2,
                        Failures = new List<DatalockEventNonPayablePeriodFailure>
                        {
                            new DatalockEventNonPayablePeriodFailure
                            {
                                ApprenticeshipId = 123,
                                Apprenticeship = new Apprenticeship(),
                                DataLockFailureId = 2,
                            },
                            new DatalockEventNonPayablePeriodFailure
                            {
                                ApprenticeshipId = 123,
                                Apprenticeship = new Apprenticeship(),
                                DataLockFailureId = 3,
                            },
                        }
                    },
                },
                PayablePeriods = new List<DatalockEventPayablePeriod>
                {
                    new DatalockEventPayablePeriod
                    {
                        Apprenticeship = new Apprenticeship(),
                        DeliveryPeriod = 1,
                    },
                },
            });

            _testInput.Add(new DatalockEvent
            {
                AcademicYear = 2021,
                PathwayCode = 1,
                StandardCode = 2,
                FrameworkCode = 3,
                ProgrammeType = 4,
                LearningAimReference = "123",
                PriceEpisodes = new List<DatalockEventPriceEpisode>
                {
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 10000m,
                        TotalNegotiatedPrice2 = 20000m,
                        InstalmentAmount = 200m,
                        NumberOfInstalments = 500,
                        CompletionAmount = 100m,
                        PriceEpisodeIdentifier = "3-3-3",
                    },
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 10000m,
                        TotalNegotiatedPrice2 = 20000m,
                        InstalmentAmount = 200m,
                        NumberOfInstalments = 500,
                        CompletionAmount = 100m,
                        PriceEpisodeIdentifier = "4-4-4",
                    }
                },
                NonPayablePeriods = new List<DatalockEventNonPayablePeriod>
                {
                    new DatalockEventNonPayablePeriod
                    {
                        DeliveryPeriod = 200,
                        Failures = new List<DatalockEventNonPayablePeriodFailure>
                        {
                            new DatalockEventNonPayablePeriodFailure
                            {
                                ApprenticeshipId = 1230,
                                Apprenticeship = new Apprenticeship(),
                                DataLockFailureId = 200,
                            },
                            new DatalockEventNonPayablePeriodFailure
                            {
                                ApprenticeshipId = 1230,
                                Apprenticeship = new Apprenticeship(),
                                DataLockFailureId = 250,
                            },
                        }
                    },
                },
                PayablePeriods = new List<DatalockEventPayablePeriod>
                {
                    new DatalockEventPayablePeriod
                    {
                        Apprenticeship = new Apprenticeship(),
                        DeliveryPeriod = 100,
                    },
                },
            });
        }

        [Test]
        public void Training_Should_HaveOneElement()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(_testInput);
            actual.Training.Should().HaveCount(1);
        }

        [Test]
        public void Training_Should_HaveExpectedPriceEpisodes()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(_testInput);
            actual.Training.First().PriceEpisodes.Should().HaveCount(4);
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "1-1-1"});
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "2-2-2"});
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "3-3-3"});
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "4-4-4"});
        }

        [Test]
        public void Training_Should_BeInOutput()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(_testInput);
            
            actual.Training.Should().ContainEquivalentOf(new {PathwayCode = 1});
        }

        [Test]
        public void ThereShouldBe_NoMixingOfPeriodsBetweenPriceEpisodes()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(_testInput);

            var firstEvent = actual.Training.FirstOrDefault(x => x.PathwayCode == 1);
            firstEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(9);
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 2 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 1 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 12 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 10 });
        }

        [Test]
        public void EventsFromSameApprenticeships_Should_BeGrouped()
        {
            var testInput = new List<DatalockEvent>
            {
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                },
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                }
            };

            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(1);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Reference__Should_NotBeGrouped()
        {
            var testInput = new List<DatalockEvent>
            {
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "1234",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                },
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                }
            };

            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Pathway__Should_NotBeGrouped()
        {
            var testInput = new List<DatalockEvent>
            {
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 11,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                },
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                }
            };

            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Standard__Should_NotBeGrouped()
        {
            var testInput = new List<DatalockEvent>
            {
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 21,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                },
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                }
            };

            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Framework__Should_NotBeGrouped()
        {
            var testInput = new List<DatalockEvent>
            {
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 31,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                },
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                }
            };

            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void EventsFromDifferentApprenticeships_Programme__Should_NotBeGrouped()
        {
            var testInput = new List<DatalockEvent>
            {
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 31,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                },
                new DatalockEvent
                {
                    AcademicYear = 2021,
                    PathwayCode = 1,
                    StandardCode = 2,
                    FrameworkCode = 3,
                    ProgrammeType = 4,
                    LearningAimReference = "123",
                    PriceEpisodes = new List<DatalockEventPriceEpisode>(),
                }
            };

            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);

            actual.Training.Should().HaveCount(2);
        }
    }
}

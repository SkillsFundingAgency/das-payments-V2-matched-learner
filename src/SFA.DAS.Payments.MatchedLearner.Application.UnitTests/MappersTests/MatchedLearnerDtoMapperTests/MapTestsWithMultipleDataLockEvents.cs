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
        private readonly MatchedLearnerDataLockInfo _testInput = new MatchedLearnerDataLockInfo();

        [SetUp]
        public void Setup()
        {
            var event1 = Guid.NewGuid();

            var event2 = Guid.NewGuid();

            _testInput.DataLockEvents = new List<DataLockEventModel>
            {
                new DataLockEventModel
                {
                    AcademicYear = 1920,
                    CollectionPeriod = 14,
                    LearningAimPathwayCode = 1,
                    EventId = event1,
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
            var nonPayableEventId1 = Guid.NewGuid();
            _testInput.DataLockEventNonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
            {
                new DataLockEventNonPayablePeriodModel
                {
                    DataLockEventId = event1,
                    DataLockEventNonPayablePeriodId = nonPayableEventId1,
                    PriceEpisodeIdentifier = "1-1-01/08/2020",
                    DeliveryPeriod = 2,
                }
            };
            _testInput.DataLockEventNonPayablePeriodFailures = new List<DataLockEventNonPayablePeriodFailureModel>
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
                AcademicYear = 2021,
                CollectionPeriod = 1,
                LearningAimPathwayCode = 2,
                EventId = event2,
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
                }
            });
            _testInput.DataLockEventPayablePeriods.AddRange(new List<DataLockEventPayablePeriodModel>
            {
                new DataLockEventPayablePeriodModel
                {
                    DataLockEventId = event2,
                    PriceEpisodeIdentifier = "4-4-01/08/2020",
                    DeliveryPeriod = 100,
                }
            });
        }

        [Test]
        public void InputWithPayablePeriod_Should_ProducePeriodWithIsPayableTrue()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);
            actual.Training
                .SelectMany(x => x.PriceEpisodes)
                .SelectMany(x => x.Periods)
                .Should().ContainEquivalentOf(new { IsPayable = true });
        }

        [Test]
        public void Training_Should_HaveTwoElements()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);
            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void Training_Should_HaveExpectedPriceEpisodes()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);
            actual.Training.First().PriceEpisodes.Should().HaveCount(2);
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "1-1-01/08/2020", AcademicYear = 1920, CollectionPeriod = 14 });
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "2-2-01/08/2020", AcademicYear = 1920, CollectionPeriod = 14 });

            actual.Training.Last().PriceEpisodes.Should().HaveCount(2);
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "3-3-01/08/2020", AcademicYear = 2021, CollectionPeriod = 1 });
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new { Identifier = "4-4-01/08/2020", AcademicYear = 2021, CollectionPeriod = 1 });
        }

        [Test]
        public void Training_Should_BeInOutput()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);

            actual.Training.Should().ContainEquivalentOf(new { PathwayCode = 1 });
            actual.Training.Should().ContainEquivalentOf(new { PathwayCode = 2 });
        }

        [Test]
        public void ThereShouldBe_NoMixingOfPeriodsBetweenPriceEpisodes()
        {
            var sut = new MatchedLearnerDtoMapper();

            var actual = sut.Map(_testInput);

            var firstEvent = actual.Training.FirstOrDefault(x => x.PathwayCode == 1);
            firstEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(2);
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 2 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 1 });

            var secondEvent = actual.Training.FirstOrDefault(x => x.PathwayCode == 2);
            secondEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(2);
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 200 });
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 100 });
        }
    }
}
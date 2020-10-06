using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Application.Models;
using NUnit.Framework;

namespace MatchedLearnerApi.Application.Tests.MappersTests.MatchedLearnerResultMapperTests
{
    [TestFixture]
    public class MapTestsWithMultipleDataLockEvents
    {
        private List<DatalockEvent> testInput = new List<DatalockEvent>();

        [SetUp]
        public void Setup()
        {
            testInput.Clear();
            testInput.Add(new DatalockEvent
            {
                AcademicYear = 2021,
                PathwayCode = 1,
                PriceEpisodes = new List<DatalockEventPriceEpisode>
                {
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 100m,
                        TotalNegotiatedPrice2 = 200m,
                        InstalmentAmount = 2m,
                        NumberOfInstalments = 5,
                        CompletionAmount = 1m,
                        Identifier = "1-1-1",
                        NonPayablePeriods = new List<DatalockEventNonPayablePeriod>
                        {
                            new DatalockEventNonPayablePeriod
                            {
                                Period = 2,
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
                                Period = 1,
                            },
                        },
                    },
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 1000m,
                        TotalNegotiatedPrice2 = 2000m,
                        InstalmentAmount = 20m,
                        NumberOfInstalments = 50,
                        CompletionAmount = 10m,
                        Identifier = "2-2-2",
                        NonPayablePeriods = new List<DatalockEventNonPayablePeriod>
                        {
                            new DatalockEventNonPayablePeriod
                            {
                                Period = 12,
                                Failures = new List<DatalockEventNonPayablePeriodFailure>
                                {
                                    new DatalockEventNonPayablePeriodFailure
                                    {
                                        ApprenticeshipId = 321,
                                        Apprenticeship = new Apprenticeship(),
                                        DataLockFailureId = 20,
                                    },
                                    new DatalockEventNonPayablePeriodFailure
                                    {
                                        ApprenticeshipId = 321,
                                        Apprenticeship = new Apprenticeship(),
                                        DataLockFailureId = 30,
                                    },
                                }
                            },
                        },
                        PayablePeriods = new List<DatalockEventPayablePeriod>
                        {
                            new DatalockEventPayablePeriod
                            {
                                Period = 10,
                                ApprenticeshipId = 3210,
                                Apprenticeship = new Apprenticeship(),
                            },
                        },
                    }
                }
            });

            testInput.Add(new DatalockEvent
            {
                AcademicYear = 2021,
                PathwayCode = 2,
                PriceEpisodes = new List<DatalockEventPriceEpisode>
                {
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 10000m,
                        TotalNegotiatedPrice2 = 20000m,
                        InstalmentAmount = 200m,
                        NumberOfInstalments = 500,
                        CompletionAmount = 100m,
                        Identifier = "3-3-3",
                        NonPayablePeriods = new List<DatalockEventNonPayablePeriod>
                        {
                            new DatalockEventNonPayablePeriod
                            {
                                Period = 200,
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
                                Period = 100,
                            },
                        },
                    },
                    new DatalockEventPriceEpisode
                    {
                        TotalNegotiatedPrice1 = 10000m,
                        TotalNegotiatedPrice2 = 20000m,
                        InstalmentAmount = 200m,
                        NumberOfInstalments = 500,
                        CompletionAmount = 100m,
                        Identifier = "4-4-4",
                        NonPayablePeriods = new List<DatalockEventNonPayablePeriod>
                        {
                            new DatalockEventNonPayablePeriod
                            {
                                Period = 120,
                                Failures = new List<DatalockEventNonPayablePeriodFailure>
                                {
                                    new DatalockEventNonPayablePeriodFailure
                                    {
                                        ApprenticeshipId = 3210,
                                        Apprenticeship = new Apprenticeship(),
                                        DataLockFailureId = 200,
                                    },
                                    new DatalockEventNonPayablePeriodFailure
                                    {
                                        ApprenticeshipId = 3210,
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
                                Period = 110,
                            },
                        },
                    }
                }
            });
        }

        [Test]
        public void InputWithPayablePeriod_Should_ProducePeriodWithIsPayableTrue()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);
            actual.Training
                .SelectMany(x => x.PriceEpisodes)
                .SelectMany(x => x.Periods)
                .Should().ContainEquivalentOf(new { IsPayable = true });
        }

        [Test]
        public void Training_Should_HaveTwoElements()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);
            actual.Training.Should().HaveCount(2);
        }

        [Test]
        public void Training_Should_HaveExpectedPriceEpisodes()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);
            actual.Training.First().PriceEpisodes.Should().HaveCount(2);
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "1-1-1"});
            actual.Training.First().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "2-2-2"});

            actual.Training.Last().PriceEpisodes.Should().HaveCount(2);
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "3-3-3"});
            actual.Training.Last().PriceEpisodes.Should().ContainEquivalentOf(new {Identifier = "4-4-4"});
        }

        [Test]
        public void Training_Should_BeInOutput()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);
            
            actual.Training.Should().ContainEquivalentOf(new {PathwayCode = 1});
            actual.Training.Should().ContainEquivalentOf(new {PathwayCode = 2});
        }

        [Test]
        public void ThereShouldBe_NoMixingOfPeriodsBetweenPriceEpisodes()
        {
            var sut = new MatchedLearnerResultMapper();

            var actual = sut.Map(testInput);

            var firstEvent = actual.Training.FirstOrDefault(x => x.PathwayCode == 1);
            firstEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(4);
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 2 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 1 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 12 });
            firstEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 10 });

            var secondEvent = actual.Training.FirstOrDefault(x => x.PathwayCode == 2);
            secondEvent!.PriceEpisodes.SelectMany(x => x.Periods).Should().HaveCount(4);
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 200 });
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 100 });
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 120 });
            secondEvent.PriceEpisodes.SelectMany(x => x.Periods).Should().ContainEquivalentOf(new { Period = 110 });
        }
    }
}

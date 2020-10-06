using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Application.Models;
using NUnit.Framework;

namespace MatchedLearnerApi.Application.Tests.MappersTests.MatchedLearnerResultMapperTests
{
    [TestFixture]
    public class MapTests
    {
        private List<DatalockEvent> testInput = new List<DatalockEvent>();

        [SetUp]
        public void Setup()
        {
            testInput.Add(new DatalockEvent
            {
                AcademicYear = 2021,
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
                                        Apprenticeship = new Apprenticeship
                                        {
                                            Id = 123,
                                        },
                                        DataLockFailureId = 2,
                                    },
                                    new DatalockEventNonPayablePeriodFailure
                                    {
                                        Apprenticeship = new Apprenticeship
                                        {
                                            Id = 123,
                                        },
                                        DataLockFailureId = 3,
                                    },
                                }
                            },
                        },
                        PayablePeriods = new List<DatalockEventPayablePeriod>
                        {
                            new DatalockEventPayablePeriod
                            {
                                Apprenticeship = new Apprenticeship
                                {

                                },
                                Period = 1,
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
    }
}

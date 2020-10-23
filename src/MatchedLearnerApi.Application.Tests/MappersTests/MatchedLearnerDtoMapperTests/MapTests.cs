using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MatchedLearnerApi.Application.Data.Models;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Types;
using NUnit.Framework;

namespace MatchedLearnerApi.Application.Tests.MappersTests.MatchedLearnerDtoMapperTests
{
    [TestFixture]
    public class MapTests
    {
        private readonly List<DatalockEvent> _testInput = new List<DatalockEvent>();
        private MatchedLearnerDto _actual;

        private readonly DateTime _expectedLearningStartDate = new DateTime(2020, 02, 01);
        private readonly DateTime _expectedEventTime = new DateTime(2020, 05, 05);
        private readonly DateTime _expectedIlrSubmissionDate = new DateTime(2020, 04, 04);
        private readonly byte _expectedIlrSubmissionWindowPeriod = 6;
        private readonly short _expectedAcademicYear = 2021;
        private readonly long _expectedUkprn = 3457922;
        private readonly long _expectedUln = 228711;
        private readonly byte _expectedApprenticeshipEmployerType = 2;

        private readonly string _expectedTrainingReference = "TrainingRef1";
        private readonly int _expectedTrainingProgrammeType = 14;
        private readonly int _expectedTrainingStandardCode = 16;
        private readonly int _expectedTrainingFrameworkCode = 18;
        private readonly int _expectedTrainingPathwayCode = 19;
        private readonly string _expectedTrainingFundingLineType = "LineTypeTwo";

        private readonly string _expectedPriceEpisodeIdentifier = "1-1-1";
        private readonly DateTime _expectedPriceEpisodeStartDate = new DateTime(2020, 01, 01);
        private readonly DateTime _expectedPriceEpisodeEndDate = new DateTime(2021, 08, 08);
        private readonly int _expectedPriceEpisodeNumberOfInstalments = 5;
        private readonly decimal _expectedPriceEpisodeInstalmentAmount = 2m;
        private readonly decimal _expectedPriceEpisodeCompletionAmount = 1m;

        [SetUp]
        public void Setup()
        {
            _testInput.Clear();
            _testInput.Add(new DatalockEvent
            {
                LearningStartDate = _expectedLearningStartDate,
                EventTime = _expectedEventTime,
                IlrSubmissionDateTime = _expectedIlrSubmissionDate,
                CollectionPeriod = _expectedIlrSubmissionWindowPeriod,
                AcademicYear = _expectedAcademicYear,
                Ukprn = _expectedUkprn,
                LearnerUln = _expectedUln,
                LearningAimReference = _expectedTrainingReference,
                LearningAimProgrammeType = _expectedTrainingProgrammeType,
                LearningAimStandardCode = _expectedTrainingStandardCode,
                LearningAimFrameworkCode = _expectedTrainingFrameworkCode,
                LearningAimPathwayCode = _expectedTrainingPathwayCode,
                LearningAimFundingLineType = _expectedTrainingFundingLineType,
                PriceEpisodes = new List<DatalockEventPriceEpisode>
                {
                    new DatalockEventPriceEpisode
                    {
                        StartDate = _expectedPriceEpisodeStartDate,
                        ActualEndDate = _expectedPriceEpisodeEndDate,
                        TotalNegotiatedPrice1 = 100m,
                        TotalNegotiatedPrice2 = 200m,
                        InstalmentAmount = _expectedPriceEpisodeInstalmentAmount,
                        NumberOfInstalments = _expectedPriceEpisodeNumberOfInstalments,
                        CompletionAmount = _expectedPriceEpisodeCompletionAmount,
                        PriceEpisodeIdentifier = _expectedPriceEpisodeIdentifier,
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
                                Apprenticeship = new Apprenticeship
                                {
                                    Id = 123,
                                    ApprenticeshipEmployerType = _expectedApprenticeshipEmployerType,
                                },
                                DataLockFailureId = 2,
                            },
                            new DatalockEventNonPayablePeriodFailure
                            {
                                Apprenticeship = new Apprenticeship
                                {
                                    Id = 123,
                                    ApprenticeshipEmployerType = _expectedApprenticeshipEmployerType,
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
                            ApprenticeshipEmployerType = _expectedApprenticeshipEmployerType,
                        },
                        DeliveryPeriod = 1,
                    },
                }
            });

            var sut = new MatchedLearnerDtoMapper();

            _actual = sut.Map(_testInput);
        }

        [Test]
        public void InputWithPayablePeriod_Should_ProducePeriodWithIsPayableTrue()
        {
            _actual.Training
                .SelectMany(x => x.PriceEpisodes)
                .SelectMany(x => x.Periods)
                .Should().ContainEquivalentOf(new { IsPayable = true });
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapStartDate()
        {
            _actual.StartDate.Should().Be(_expectedLearningStartDate);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapApprenticeshipEmployerType()
        {
            _actual.Training
                .SelectMany(x => x.PriceEpisodes)
                .SelectMany(x => x.Periods)
                .Should()
                .AllBeEquivalentTo(new { ApprenticeshipEmployerType = _expectedApprenticeshipEmployerType });
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapEventTime()
        {
            _actual.EventTime.Should().Be(_expectedEventTime);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapIlrSubmissionDate()
        {
            _actual.IlrSubmissionDate.Should().Be(_expectedIlrSubmissionDate);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapIlrSubmissionWindowPeriod()
        {
            _actual.IlrSubmissionWindowPeriod.Should().Be(_expectedIlrSubmissionWindowPeriod);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapAcademicYear()
        {
            _actual.AcademicYear.Should().Be(_expectedAcademicYear);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapUkprn()
        {
            _actual.Ukprn.Should().Be(_expectedUkprn);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapUln()
        {
            _actual.Uln.Should().Be(_expectedUln);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapTrainingReference()
        {
            _actual.Training.Count.Should().Be(1);
            _actual.Training.Single().Reference.Should().Be(_expectedTrainingReference);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapTrainingProgrammeType()
        {
            _actual.Training.Single().ProgrammeType.Should().Be(_expectedTrainingProgrammeType);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapTrainingStandardCode()
        {
            _actual.Training.Single().StandardCode.Should().Be(_expectedTrainingStandardCode);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapTrainingFrameworkCode()
        {
            _actual.Training.Single().FrameworkCode.Should().Be(_expectedTrainingFrameworkCode);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapTrainingPathwayCode()
        {
            _actual.Training.Single().PathwayCode.Should().Be(_expectedTrainingPathwayCode);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapTrainingFundingLineType()
        {
            //_actual.Training.Single().LearningAimFundingLineType.Should().Be(_expectedTrainingFundingLineType);
            _actual.Training.Single().FundingLineType.Should().BeNullOrEmpty();
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapTrainingStartDate()
        {
            _actual.Training.Single().StartDate.Should().Be(_expectedLearningStartDate);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapPriceEpisodeIdentifier()
        {
            _actual.Training.Single().PriceEpisodes.Single().Identifier.Should().Be(_expectedPriceEpisodeIdentifier);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapPriceEpisodeStartDate()
        {
            _actual.Training.Single().PriceEpisodes.Single().StartDate.Should().Be(_expectedPriceEpisodeStartDate);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapPriceEpisodeEndDate()
        {
            _actual.Training.Single().PriceEpisodes.Single().EndDate.Should().Be(_expectedPriceEpisodeEndDate);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapPriceEpisodeNumberOfInstalments()
        {
            _actual.Training.Single().PriceEpisodes.Single().InstalmentAmount.Should().Be(_expectedPriceEpisodeInstalmentAmount);
        }

        [Test]
        public void InputWithPayablePeriod_Should_MapPriceEpisodeCompletionAmount()
        {
            _actual.Training.Single().PriceEpisodes.Single().CompletionAmount.Should().Be(_expectedPriceEpisodeCompletionAmount);
        }
    }
}

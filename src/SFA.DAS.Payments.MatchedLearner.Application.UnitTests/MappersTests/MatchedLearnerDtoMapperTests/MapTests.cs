using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.MappersTests.MatchedLearnerDtoMapperTests
{
    [TestFixture]
    public class MapTests
    {
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

        private readonly string _expectedPriceEpisodeIdentifier = "1-1-01/08/2020";
        private readonly DateTime _expectedPriceEpisodeIdentifierDatePart = new DateTime(2020,08,01);
        private readonly DateTime _expectedPriceEpisodeStartDate = new DateTime(2020, 02, 01);
        private readonly DateTime _expectedPriceEpisodeEndDate = new DateTime(2021, 08, 08);
        private readonly int _expectedPriceEpisodeNumberOfInstalments = 5;
        private readonly decimal _expectedPriceEpisodeInstalmentAmount = 2m;
        private readonly decimal _expectedPriceEpisodeCompletionAmount = 1m;
        private readonly DateTime _expectedTotalNegotiatedPriceStartDate = new DateTime(2020, 02, 01);

        [SetUp]
        public void Setup()
        {
            var testInput = new MatchedLearnerDataLockInfo
            {
                //TODO: Fix this
                //LatestSuccessfulJobs = new List<LatestSuccessfulJobModel>
                //{
                //    new LatestSuccessfulJobModel
                //    {
                //        CollectionPeriod = _expectedIlrSubmissionWindowPeriod,
                //        AcademicYear = _expectedAcademicYear,
                //        IlrSubmissionTime = _expectedIlrSubmissionDate,
                //        Ukprn = _expectedUkprn,
                //        JobId = 1,
                //        DcJobId = 1,
                //    }
                //},
                DataLockEvents = new List<DataLockEventModel>
                {
                    new DataLockEventModel
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
                    }
                },

                DataLockEventPriceEpisodes = new List<DataLockEventPriceEpisodeModel>
                {
                    new DataLockEventPriceEpisodeModel
                    {
                        StartDate = _expectedPriceEpisodeStartDate,
                        ActualEndDate = _expectedPriceEpisodeEndDate,
                        TotalNegotiatedPrice1 = 100m,
                        TotalNegotiatedPrice2 = 200m,
                        InstalmentAmount = _expectedPriceEpisodeInstalmentAmount,
                        NumberOfInstalments = _expectedPriceEpisodeNumberOfInstalments,
                        CompletionAmount = _expectedPriceEpisodeCompletionAmount,
                        PriceEpisodeIdentifier = _expectedPriceEpisodeIdentifier,
                        EffectiveTotalNegotiatedPriceStartDate = _expectedTotalNegotiatedPriceStartDate
                    }
                },

                DataLockEventNonPayablePeriods = new List<DataLockEventNonPayablePeriodModel>
                {
                    new DataLockEventNonPayablePeriodModel
                    {
                        PriceEpisodeIdentifier = _expectedPriceEpisodeIdentifier,
                        DeliveryPeriod = 2,
                    }
                },

                DataLockEventNonPayablePeriodFailures = new List<DataLockEventNonPayablePeriodFailureModel>
                {
                    new DataLockEventNonPayablePeriodFailureModel
                    {
                        ApprenticeshipId = 123,
                        DataLockFailureId = 2,
                    },
                    new DataLockEventNonPayablePeriodFailureModel
                    {
                        ApprenticeshipId = 123,
                        DataLockFailureId = 3,
                    },
                },

                Apprenticeships = new List<ApprenticeshipModel>
                {
                    new ApprenticeshipModel
                    {
                        Id = 123,
                        ApprenticeshipEmployerType = _expectedApprenticeshipEmployerType
                    },
                    new ApprenticeshipModel
                    {
                        Id = 456,
                        ApprenticeshipEmployerType = _expectedApprenticeshipEmployerType
                    }
                },

                DataLockEventPayablePeriods = new List<DataLockEventPayablePeriodModel>
                {
                    new DataLockEventPayablePeriodModel
                    {
                        PriceEpisodeIdentifier = _expectedPriceEpisodeIdentifier,
                        ApprenticeshipId = 456,
                        DeliveryPeriod = 1,
                    },
                }
            };

            var sut = new MatchedLearnerDtoMapper();

            _actual = sut.Map(testInput);
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
            _actual.Training.Single().PriceEpisodes.Single().StartDate.Should().Be(_expectedPriceEpisodeIdentifierDatePart);
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

        [Test]
        public void InputWithPayablePeriod_Should_MapTotalNegotiatedPriceStartDate()
        {
            _actual.Training.Single().PriceEpisodes.Single().TotalNegotiatedPriceStartDate.Should().Be(_expectedTotalNegotiatedPriceStartDate);
        }
    }
}
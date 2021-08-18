using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Application.Mappers
{
    public interface IMatchedLearnerDtoMapper
    {
        MatchedLearnerDto Map(MatchedLearnerDataLockInfo matchedLearnerDataLockInfo);
    }

    public class MatchedLearnerDtoMapper : IMatchedLearnerDtoMapper
    {
        public MatchedLearnerDto Map(MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
        {
            if (matchedLearnerDataLockInfo == null || !matchedLearnerDataLockInfo.DataLockEvents.Any())
                return null;

            var firstEvent = matchedLearnerDataLockInfo.DataLockEvents
                .OrderByDescending(x => x.AcademicYear)
                .ThenBy(x => x.LearningStartDate)
                .First();

            return new MatchedLearnerDto
            {
                StartDate = firstEvent.LearningStartDate.GetValueOrDefault(),
                EventTime = firstEvent.EventTime,
                IlrSubmissionDate = firstEvent.IlrSubmissionDateTime,
                IlrSubmissionWindowPeriod = firstEvent.CollectionPeriod,
                AcademicYear = firstEvent.AcademicYear,
                Ukprn = firstEvent.Ukprn,
                Uln = firstEvent.LearnerUln,
                Training = MapTraining(matchedLearnerDataLockInfo)
            };
        }

        private static List<TrainingDto> MapTraining(MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
        {
            return matchedLearnerDataLockInfo.DataLockEvents.GroupBy(x => new
            {
                x.LearningAimReference,
                x.LearningAimStandardCode,
                x.LearningAimProgrammeType,
                x.LearningAimFrameworkCode,
                x.LearningAimPathwayCode,
                x.LearningAimFundingLineType,
                x.LearningStartDate,
                x.LearnerUln,
                x.Ukprn
            }).Select(dataLockEvent => new TrainingDto
            {
                Reference = dataLockEvent.Key.LearningAimReference,
                ProgrammeType = dataLockEvent.Key.LearningAimProgrammeType,
                StandardCode = dataLockEvent.Key.LearningAimStandardCode,
                FrameworkCode = dataLockEvent.Key.LearningAimFrameworkCode,
                PathwayCode = dataLockEvent.Key.LearningAimPathwayCode,
                FundingLineType = null,
                StartDate = dataLockEvent.Key.LearningStartDate.GetValueOrDefault(),
                PriceEpisodes = MapPriceEpisodes(dataLockEvent.Select(d => d.EventId), matchedLearnerDataLockInfo)
            }).ToList();
        }

        private static List<PriceEpisodeDto> MapPriceEpisodes(IEnumerable<Guid> dataLockEventIds, MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
        {
            return matchedLearnerDataLockInfo
                .DataLockEventPriceEpisodes.Where(p => dataLockEventIds.Contains(p.DataLockEventId))
                .GroupBy(x => new
                {
                    x.PriceEpisodeIdentifier,
                    x.AgreedPrice,
                    x.StartDate,
                    x.ActualEndDate,
                    x.NumberOfInstalments,
                    x.InstalmentAmount,
                    x.CompletionAmount,
                    x.EffectiveTotalNegotiatedPriceStartDate
                })
                .Select(priceEpisode =>
                {
                    var dataLockEvent = matchedLearnerDataLockInfo.DataLockEvents.Single(d =>
                            priceEpisode.First().DataLockEventId == d.EventId);

                    return new PriceEpisodeDto
                    {
                        AcademicYear = dataLockEvent.AcademicYear,
                        CollectionPeriod = dataLockEvent.CollectionPeriod,
                        Identifier = priceEpisode.Key.PriceEpisodeIdentifier,
                        AgreedPrice = priceEpisode.Key.AgreedPrice,
                        StartDate = ExtractEpisodeStartDateFromPriceEpisodeIdentifier(priceEpisode.Key.PriceEpisodeIdentifier),
                        EndDate = priceEpisode.Key.ActualEndDate,
                        NumberOfInstalments = priceEpisode.Key.NumberOfInstalments,
                        InstalmentAmount = priceEpisode.Key.InstalmentAmount,
                        CompletionAmount = priceEpisode.Key.CompletionAmount,
                        Periods = MapPeriods(priceEpisode.Key.PriceEpisodeIdentifier, matchedLearnerDataLockInfo),
                        TotalNegotiatedPriceStartDate = priceEpisode.Key.EffectiveTotalNegotiatedPriceStartDate
                    };
                }).ToList();
        }

        private static DateTime ExtractEpisodeStartDateFromPriceEpisodeIdentifier(string priceEpisodeIdentifier)
        {
            return DateTime.TryParseExact(
                   priceEpisodeIdentifier.Substring(priceEpisodeIdentifier.Length - 10),
                    "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces,
                    out var episodeStartDate)
                ? episodeStartDate
                : throw new InvalidOperationException(
                    $"Cannot determine episode start date from the price episode identifier: {priceEpisodeIdentifier}");
        }

        private static List<PeriodDto> MapPeriods(string priceEpisodeIdentifier, MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
        {
            var nonPayablePeriods = matchedLearnerDataLockInfo.DataLockEventNonPayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .Select(nonPayablePeriod =>
                {
                    var failures = matchedLearnerDataLockInfo.DataLockEventNonPayablePeriodFailures
                        .Where(f => f.DataLockEventNonPayablePeriodId ==
                                    nonPayablePeriod.DataLockEventNonPayablePeriodId).ToList();

                    var apprenticeship = matchedLearnerDataLockInfo.Apprenticeships.FirstOrDefault(a => a.Id == failures.First().ApprenticeshipId);

                    return new PeriodDto
                    {
                        Period = nonPayablePeriod.DeliveryPeriod,
                        IsPayable = false,
                        DataLockFailures = failures.Select(f => (int)f.DataLockFailureId).ToList(),
                        AccountId = apprenticeship?.AccountId ?? 0,
                        ApprenticeshipId = apprenticeship?.Id,
                        ApprenticeshipEmployerType = apprenticeship?.ApprenticeshipEmployerType ?? 0,
                        TransferSenderAccountId = apprenticeship?.TransferSendingEmployerAccountId ?? 0
                    };
                });

            var payablePeriods = matchedLearnerDataLockInfo.DataLockEventPayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .Select(payablePeriod =>
                {
                    var apprenticeship = matchedLearnerDataLockInfo.Apprenticeships.FirstOrDefault(a => a.Id == payablePeriod.ApprenticeshipId);

                    return new PeriodDto
                    {
                        Period = payablePeriod.DeliveryPeriod,
                        IsPayable = true,
                        AccountId = apprenticeship?.AccountId ?? 0,
                        ApprenticeshipId = apprenticeship?.Id,
                        ApprenticeshipEmployerType = apprenticeship?.ApprenticeshipEmployerType ?? 0,
                        TransferSenderAccountId = apprenticeship?.TransferSendingEmployerAccountId ?? 0,
                    };
                });

            return payablePeriods.Union(nonPayablePeriods).ToList();
        }
    }
}
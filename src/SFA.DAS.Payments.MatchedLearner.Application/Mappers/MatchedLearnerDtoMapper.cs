﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Application.Mappers
{
    public interface IMatchedLearnerDtoMapper
    {
        MatchedLearnerDto MapToDto(TrainingModel training);
        List<TrainingModel> MapToModel(List<DataLockEventModel> dataLockEvents, List<ApprenticeshipModel> apprenticeshipModels);
    }

    public class MatchedLearnerDtoMapper : IMatchedLearnerDtoMapper
    {
        public MatchedLearnerDto MapToDto(TrainingModel training)
        {
            throw new NotImplementedException();
        }

        public List<TrainingModel> MapToModel(List<DataLockEventModel> dataLockEvents, List<ApprenticeshipModel> apprenticeshipModels)
        {
            return new List<TrainingModel>();

            //var transactionTypes = new List<byte> { 1, 2, 3 };

            //var result = dataLockEvents
            //    .GroupBy(grp => grp.LearnerUln)
            //    .Select(groupedDataLocks =>
            //    {
            //        var dataLocks = groupedDataLocks.Where(x => x.LearningAimReference == "ZPROG001").ToList();

            //        var eventIds = dataLocks.Select(dl => dl.EventId).ToList();

            //        var dataLockEventPriceEpisodes = dataLocks.SelectMany(dl => dl.PriceEpisodes)
            //            .Where(d => eventIds.Contains(d.DataLockEventId) && d.PriceEpisodeIdentifier != null)
            //            .OrderBy(p => p.StartDate)
            //            .ThenBy(p => p.PriceEpisodeIdentifier)
            //            .ToList();

            //        var dataLockEventPayablePeriods = dataLocks.SelectMany(dl => dl.PayablePeriods)
            //            .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
            //            .OrderBy(p => p.DeliveryPeriod)
            //            .ToList();

            //        var dataLockEventNonPayablePeriods = dataLocks.SelectMany(dl => dl.NonPayablePeriods)
            //            .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
            //            .OrderBy(p => p.DeliveryPeriod)
            //            .ToList();

            //        var dataLockEventNonPayablePeriodFailures = dataLockEventNonPayablePeriods.SelectMany(d => d.Failures).ToList();

            //        return new MatchedLearnerDataLockInfo
            //        {
            //            DataLockEvents = dataLocks.Select(i =>
            //            {
            //                i.NonPayablePeriods = null;
            //                i.PayablePeriods = null;
            //                i.PriceEpisodes = null;
            //                return i;
            //            }).ToList(),
            //            DataLockEventPriceEpisodes = dataLockEventPriceEpisodes,
            //            DataLockEventPayablePeriods = dataLockEventPayablePeriods,
            //            DataLockEventNonPayablePeriods = dataLockEventNonPayablePeriods.Select(i =>
            //            {
            //                i.Failures = null;
            //                return i;
            //            }).ToList(),
            //            DataLockEventNonPayablePeriodFailures = dataLockEventNonPayablePeriodFailures
            //        };
            //    })
            //    .ToList();

            //return result;
        }

        public MatchedLearnerDto MapToDto(MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
        {
            if (matchedLearnerDataLockInfo == null || !matchedLearnerDataLockInfo.DataLockEvents.Any())
                return null;

            var firstEvent = matchedLearnerDataLockInfo.DataLockEvents
                .OrderByDescending(x => x.AcademicYear)
                .ThenByDescending(x => x.CollectionPeriod)
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
                })
                .OrderByDescending(x => x.AcademicYear)
                .ThenByDescending(x => x.CollectionPeriod)
                .ToList();
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
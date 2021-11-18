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
            var transactionTypes = new List<byte> { 1, 2, 3 };

            var result = dataLockEvents
                .GroupBy(grp => grp.LearnerUln)
                .Select(groupedDataLocks =>
                {
                    var dataLocks = groupedDataLocks.Where(x => x.LearningAimReference == "ZPROG001").ToList();

                    var eventIds = dataLocks.Select(dl => dl.EventId).ToList();

                    var dataLockEventPriceEpisodes = dataLocks.SelectMany(dl => dl.PriceEpisodes)
                        .Where(d => eventIds.Contains(d.DataLockEventId) && d.PriceEpisodeIdentifier != null)
                        .ToList();

                    var dataLockEventPayablePeriods = dataLocks.SelectMany(dl => dl.PayablePeriods)
                        .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                        .ToList();

                    var dataLockEventNonPayablePeriods = dataLocks.SelectMany(dl => dl.NonPayablePeriods)
                        .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                        .ToList();

                    var dataLockEventNonPayablePeriodFailures = dataLockEventNonPayablePeriods.SelectMany(d => d.Failures).ToList();

                    var matchedLearnerInfo = new MatchedLearnerDataLockInfo
                    {
                        DataLockEvents = dataLocks.Select(i =>
                        {
                            i.NonPayablePeriods = null;
                            i.PayablePeriods = null;
                            i.PriceEpisodes = null;
                            return i;
                        }).ToList(),
                        DataLockEventPriceEpisodes = dataLockEventPriceEpisodes,
                        DataLockEventPayablePeriods = dataLockEventPayablePeriods,
                        DataLockEventNonPayablePeriods = dataLockEventNonPayablePeriods.Select(i =>
                        {
                            i.Failures = null;
                            return i;
                        }).ToList(),
                        DataLockEventNonPayablePeriodFailures = dataLockEventNonPayablePeriodFailures,
                        Apprenticeships = apprenticeshipModels
                    };

                    return MapTraining(matchedLearnerInfo);
                })
                .SelectMany(tr => tr)
                .ToList();

            return result;
        }

        private static List<TrainingModel> MapTraining(MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
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
            }).Select(dataLockEvent =>
            {
                var firstDataLockEvent = dataLockEvent.First();
                return new TrainingModel
                {
                    EventTime = firstDataLockEvent.EventTime,
                    EventId = firstDataLockEvent.EventId,
                    IlrSubmissionDate = firstDataLockEvent.IlrSubmissionDateTime,
                    IlrSubmissionWindowPeriod = firstDataLockEvent.CollectionPeriod,
                    AcademicYear = firstDataLockEvent.AcademicYear,
                    Ukprn = firstDataLockEvent.Ukprn,
                    Uln = firstDataLockEvent.LearnerUln,
                    Reference = dataLockEvent.Key.LearningAimReference,
                    ProgrammeType = dataLockEvent.Key.LearningAimProgrammeType,
                    StandardCode = dataLockEvent.Key.LearningAimStandardCode,
                    FrameworkCode = dataLockEvent.Key.LearningAimFrameworkCode,
                    PathwayCode = dataLockEvent.Key.LearningAimPathwayCode,
                    FundingLineType = dataLockEvent.Key.LearningAimFundingLineType,
                    StartDate = dataLockEvent.Key.LearningStartDate.GetValueOrDefault(),
                    PriceEpisodes = MapPriceEpisodes(dataLockEvent.Select(d => d.EventId), matchedLearnerDataLockInfo)
                };
            }).ToList();
        }

        private static List<PriceEpisodeModel> MapPriceEpisodes(IEnumerable<Guid> dataLockEventIds, MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
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

                    return new PriceEpisodeModel
                    {
                        Identifier = priceEpisode.Key.PriceEpisodeIdentifier,
                        AcademicYear = dataLockEvent.AcademicYear,
                        CollectionPeriod = dataLockEvent.CollectionPeriod,
                        AgreedPrice = priceEpisode.Key.AgreedPrice,
                        StartDate = ExtractEpisodeStartDateFromPriceEpisodeIdentifier(priceEpisode.Key.PriceEpisodeIdentifier),
                        ActualEndDate = priceEpisode.Key.ActualEndDate,
                        PlannedEndDate = priceEpisode.First().PlannedEndDate,
                        NumberOfInstalments = priceEpisode.Key.NumberOfInstalments,
                        InstalmentAmount = priceEpisode.Key.InstalmentAmount,
                        CompletionAmount = priceEpisode.Key.CompletionAmount,
                        TotalNegotiatedPriceStartDate = priceEpisode.Key.EffectiveTotalNegotiatedPriceStartDate,
                        Periods = MapPeriods(priceEpisode.Key.PriceEpisodeIdentifier, matchedLearnerDataLockInfo)
                    };
                })
                .OrderByDescending(x => x.AcademicYear)
                .ThenByDescending(x => x.CollectionPeriod)
                .ToList();
        }

        private static DateTime ExtractEpisodeStartDateFromPriceEpisodeIdentifier(string priceEpisodeIdentifier)
        {
            return DateTime.TryParseExact(
                priceEpisodeIdentifier.Substring(priceEpisodeIdentifier.Length - 10), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var episodeStartDate)
                ? episodeStartDate
                : throw new InvalidOperationException($"Cannot determine episode start date from the price episode identifier: {priceEpisodeIdentifier}");
        }

        private static List<PeriodModel> MapPeriods(string priceEpisodeIdentifier, MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
        {
            var nonPayablePeriods = matchedLearnerDataLockInfo.DataLockEventNonPayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .Select(nonPayablePeriod =>
                {
                    var failures = matchedLearnerDataLockInfo.DataLockEventNonPayablePeriodFailures
                        .Where(f => f.DataLockEventNonPayablePeriodId ==
                                    nonPayablePeriod.DataLockEventNonPayablePeriodId).ToList();

                    var apprenticeship = matchedLearnerDataLockInfo.Apprenticeships.FirstOrDefault(a => a.Id == failures.First().ApprenticeshipId);

                    return new PeriodModel
                    {
                        IsPayable = false,

                        TransactionType = nonPayablePeriod.TransactionType,

                        Period = nonPayablePeriod.DeliveryPeriod,

                        Amount = nonPayablePeriod.Amount,

                        ApprenticeshipId = apprenticeship?.Id,
                        AccountId = apprenticeship?.AccountId ?? 0,
                        ApprenticeshipEmployerType = apprenticeship?.ApprenticeshipEmployerType ?? 0,
                        TransferSenderAccountId = apprenticeship?.TransferSendingEmployerAccountId ?? 0,

                        FailedDataLock1  = failures.FirstOrDefault(f => f.DataLockFailureId == 1) != null,
                        FailedDataLock2  = failures.FirstOrDefault(f => f.DataLockFailureId == 2) != null,
                        FailedDataLock3  = failures.FirstOrDefault(f => f.DataLockFailureId == 3) != null,
                        FailedDataLock4  = failures.FirstOrDefault(f => f.DataLockFailureId == 4) != null,
                        FailedDataLock5  = failures.FirstOrDefault(f => f.DataLockFailureId == 5) != null,
                        FailedDataLock6  = failures.FirstOrDefault(f => f.DataLockFailureId == 6) != null,
                        FailedDataLock7  = failures.FirstOrDefault(f => f.DataLockFailureId == 7) != null,
                        FailedDataLock8  = failures.FirstOrDefault(f => f.DataLockFailureId == 8) != null,
                        FailedDataLock9  = failures.FirstOrDefault(f => f.DataLockFailureId == 9) != null,
                        FailedDataLock10 = failures.FirstOrDefault(f => f.DataLockFailureId == 10) != null,
                        FailedDataLock11 = failures.FirstOrDefault(f => f.DataLockFailureId == 11) != null,
                        FailedDataLock12 = failures.FirstOrDefault(f => f.DataLockFailureId == 12) != null
                    };
                });

            var payablePeriods = matchedLearnerDataLockInfo.DataLockEventPayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .Select(payablePeriod =>
                {
                    var apprenticeship = matchedLearnerDataLockInfo.Apprenticeships.FirstOrDefault(a => a.Id == payablePeriod.ApprenticeshipId);

                    return new PeriodModel
                    {
                        IsPayable = true,

                        TransactionType = payablePeriod.TransactionType,
                        
                        Period = payablePeriod.DeliveryPeriod,
                        
                        Amount = payablePeriod.Amount,

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
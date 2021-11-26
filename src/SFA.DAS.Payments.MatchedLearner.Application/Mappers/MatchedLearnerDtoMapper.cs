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

            var result = MapTraining(dataLockEvents, apprenticeshipModels);

            return result;
        }

        private static List<TrainingModel> MapTraining(List<DataLockEventModel> dataLockEvents, List<ApprenticeshipModel> apprenticeshipModels)
        {
            return dataLockEvents
                .Where(x => x.LearningAimReference == "ZPROG001")
                .GroupBy(x => new
                {
                    x.LearningAimReference,
                    x.LearningAimStandardCode,
                    x.LearningAimProgrammeType,
                    x.LearningAimFrameworkCode,
                    x.LearningAimPathwayCode,
                    x.LearningAimFundingLineType,
                    x.LearningStartDate,
                    x.LearnerUln,
                    x.Ukprn,
                    x.AcademicYear,
                    x.CollectionPeriod
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

                        PriceEpisodes = MapPriceEpisodes(dataLockEvent.ToList(), apprenticeshipModels)
                    };
                }).ToList();
        }

        private static List<PriceEpisodeModel> MapPriceEpisodes(IList<DataLockEventModel> dataLockEvents, IList<ApprenticeshipModel> apprenticeshipModels)
        {
            var transactionTypes = new List<byte> { 1, 2, 3 };

            return dataLockEvents
                .SelectMany(d => d.PriceEpisodes)
                .Where(d => d.PriceEpisodeIdentifier != null)
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
                    var firstDataLockEvent = dataLockEvents.First();

                    var dataLockEventNonPayablePeriods = dataLockEvents
                        .SelectMany(d => d.NonPayablePeriods)
                        .Where(d => d.PriceEpisodeIdentifier == priceEpisode.Key.PriceEpisodeIdentifier && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0);

                    var dataLockEventPayablePeriods = dataLockEvents
                        .SelectMany(d => d.PayablePeriods)
                        .Where(d => d.PriceEpisodeIdentifier == priceEpisode.Key.PriceEpisodeIdentifier && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0);

                    return new PriceEpisodeModel
                    {
                        AcademicYear = firstDataLockEvent.AcademicYear,
                        CollectionPeriod = firstDataLockEvent.CollectionPeriod,

                        Identifier = priceEpisode.Key.PriceEpisodeIdentifier,
                        AgreedPrice = priceEpisode.Key.AgreedPrice,
                        ActualEndDate = priceEpisode.Key.ActualEndDate,
                        NumberOfInstalments = priceEpisode.Key.NumberOfInstalments,
                        InstalmentAmount = priceEpisode.Key.InstalmentAmount,
                        CompletionAmount = priceEpisode.Key.CompletionAmount,
                        TotalNegotiatedPriceStartDate = priceEpisode.Key.EffectiveTotalNegotiatedPriceStartDate,

                        PlannedEndDate = priceEpisode.First().PlannedEndDate,

                        StartDate = ExtractEpisodeStartDateFromPriceEpisodeIdentifier(priceEpisode.Key.PriceEpisodeIdentifier),

                        Periods = MapPeriods(dataLockEventNonPayablePeriods, dataLockEventPayablePeriods, apprenticeshipModels)
                    };
                })
                .ToList();
        }

        private static DateTime ExtractEpisodeStartDateFromPriceEpisodeIdentifier(string priceEpisodeIdentifier)
        {
            return DateTime.TryParseExact(
                priceEpisodeIdentifier.Substring(priceEpisodeIdentifier.Length - 10), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var episodeStartDate)
                ? episodeStartDate
                : throw new InvalidOperationException($"Cannot determine episode start date from the price episode identifier: {priceEpisodeIdentifier}");
        }

        private static List<PeriodModel> MapPeriods(IEnumerable<DataLockEventNonPayablePeriodModel> dataLockEventNonPayablePeriods, IEnumerable<DataLockEventPayablePeriodModel> dataLockEventPayablePeriods, IList<ApprenticeshipModel> apprenticeshipModels)
        {
            var nonPayablePeriods = dataLockEventNonPayablePeriods
                .Select(nonPayablePeriod =>
                {
                    var failures = nonPayablePeriod.Failures;

                    var apprenticeship = !failures.Any() ? null : apprenticeshipModels.FirstOrDefault(a => a.Id == failures.First().ApprenticeshipId);

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

            var payablePeriods = dataLockEventPayablePeriods
                .Select(payablePeriod =>
                {
                    var apprenticeship = apprenticeshipModels.FirstOrDefault(a => a.Id == payablePeriod.ApprenticeshipId);

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
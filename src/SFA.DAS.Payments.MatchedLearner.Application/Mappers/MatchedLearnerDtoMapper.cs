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
        MatchedLearnerDto MapToDto(List<TrainingModel> trainings);
        List<TrainingModel> MapToModel(List<DataLockEventModel> dataLockEvents, List<ApprenticeshipModel> apprenticeshipModels);
    }

    public class MatchedLearnerDtoMapper : IMatchedLearnerDtoMapper
    {
        public MatchedLearnerDto MapToDto(List<TrainingModel> trainings)
        {
            var firstTraining = trainings
                .OrderByDescending(t => t.AcademicYear)
                .ThenByDescending(t => t.IlrSubmissionWindowPeriod)
                .First();

            var result = new MatchedLearnerDto
            {
                AcademicYear = firstTraining.AcademicYear,
                IlrSubmissionWindowPeriod = firstTraining.IlrSubmissionWindowPeriod,
                IlrSubmissionDate = firstTraining.IlrSubmissionDate,
                Ukprn = firstTraining.Ukprn,
                Uln = firstTraining.Uln,
                EventTime = firstTraining.EventTime,
                StartDate = firstTraining.StartDate,
                Training = trainings.GroupBy(x => new
                {
                    x.Reference,
                    x.StandardCode,
                    x.ProgrammeType,
                    x.FrameworkCode,
                    x.PathwayCode,
                    x.FundingLineType,
                    x.StartDate,
                    x.Uln,
                    x.Ukprn,
                }).Select(trainingGrp =>
                {
                    var currentTrainings = trainingGrp.ToList();
                    return new TrainingDto
                    {
                        Reference = trainingGrp.Key.Reference,
                        StandardCode = trainingGrp.Key.StandardCode,
                        ProgrammeType = trainingGrp.Key.ProgrammeType,
                        FrameworkCode = trainingGrp.Key.FrameworkCode,
                        PathwayCode = trainingGrp.Key.PathwayCode,
                        FundingLineType = null,
                        StartDate = trainingGrp.Key.StartDate,
                        PriceEpisodes = trainingGrp.SelectMany(tg => tg.PriceEpisodes).GroupBy(x => new
                        {
                            x.Identifier,
                            x.AgreedPrice,
                            x.StartDate,
                            x.ActualEndDate,
                            x.NumberOfInstalments,
                            x.InstalmentAmount,
                            x.CompletionAmount,
                            x.TotalNegotiatedPriceStartDate
                        }).Select(priceEpisode =>
                        {
                            var firstDataLockEvent = currentTrainings
                                .OrderByDescending(t => t.AcademicYear)
                                .ThenByDescending(t => t.IlrSubmissionWindowPeriod)
                                .First();

                            return new PriceEpisodeDto
                            {
                                AcademicYear = firstDataLockEvent.AcademicYear,
                                CollectionPeriod = firstDataLockEvent.IlrSubmissionWindowPeriod,

                                Identifier = priceEpisode.Key.Identifier,
                                AgreedPrice = priceEpisode.Key.AgreedPrice,
                                StartDate = priceEpisode.Key.StartDate,
                                EndDate = priceEpisode.Key.ActualEndDate,
                                NumberOfInstalments = priceEpisode.Key.NumberOfInstalments,
                                InstalmentAmount = priceEpisode.Key.InstalmentAmount,
                                CompletionAmount = priceEpisode.Key.CompletionAmount,
                                TotalNegotiatedPriceStartDate = priceEpisode.Key.TotalNegotiatedPriceStartDate,

                                Periods = priceEpisode.SelectMany(p => p.Periods).Select(pd =>
                                {
                                    var failures = new List<int>();

                                    if (!pd.IsPayable)
                                    {
                                        if (pd.FailedDataLock1) failures.Add(1);
                                        if (pd.FailedDataLock2) failures.Add(2);
                                        if (pd.FailedDataLock3) failures.Add(3);
                                        if (pd.FailedDataLock4) failures.Add(4);
                                        if (pd.FailedDataLock5) failures.Add(5);
                                        if (pd.FailedDataLock6) failures.Add(6);
                                        if (pd.FailedDataLock7) failures.Add(7);
                                        if (pd.FailedDataLock8) failures.Add(8);
                                        if (pd.FailedDataLock9) failures.Add(9);
                                        if (pd.FailedDataLock10) failures.Add(10);
                                        if (pd.FailedDataLock11) failures.Add(11);
                                        if (pd.FailedDataLock12) failures.Add(12);
                                    }

                                    return new PeriodDto
                                    {
                                        AccountId = pd.AccountId ?? 0,
                                        ApprenticeshipId = pd.ApprenticeshipId ?? 0,
                                        TransferSenderAccountId = pd.TransferSenderAccountId ?? 0,
                                        ApprenticeshipEmployerType = pd.ApprenticeshipEmployerType ?? 0,
                                        IsPayable = pd.IsPayable,
                                        Period = pd.Period,
                                        DataLockFailures = failures
                                    };
                                }).ToList()
                            };
                        }).ToList()
                    };
                }).ToList()
            };

            return result;
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
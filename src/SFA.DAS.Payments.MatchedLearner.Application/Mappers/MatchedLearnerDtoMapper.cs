using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.MatchedLearner.Application.Data;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Application.Mappers
{
    public interface IMatchedLearnerDtoMapper
    {
        MatchedLearnerDto Map(MatchedLearnerDataLockDataDto matchedLearnerDataLockData);
    }

    public class MatchedLearnerDtoMapper : IMatchedLearnerDtoMapper
    {
        public MatchedLearnerDto Map(MatchedLearnerDataLockDataDto matchedLearnerDataLockData)
        {
            if (matchedLearnerDataLockData == null || !matchedLearnerDataLockData.DataLockEvents.Any())
                return null;

            var firstEvent = matchedLearnerDataLockData.DataLockEvents.First();
            
            return new MatchedLearnerDto
            {
                StartDate = firstEvent.LearningStartDate.GetValueOrDefault(),
                EventTime = firstEvent.EventTime,
                IlrSubmissionDate = firstEvent.IlrSubmissionDateTime,
                IlrSubmissionWindowPeriod = firstEvent.CollectionPeriod,
                AcademicYear = firstEvent.AcademicYear,
                Ukprn = firstEvent.Ukprn,
                Uln = firstEvent.LearnerUln,
                Training = matchedLearnerDataLockData.DataLockEvents.Select(dataLockEvent => new TrainingDto
                {
                    Reference = dataLockEvent.LearningAimReference,
                    ProgrammeType = dataLockEvent.LearningAimProgrammeType,
                    StandardCode = dataLockEvent.LearningAimStandardCode,
                    FrameworkCode = dataLockEvent.LearningAimFrameworkCode,
                    PathwayCode = dataLockEvent.LearningAimPathwayCode,
                    FundingLineType = null,
                    StartDate = dataLockEvent.LearningStartDate.GetValueOrDefault(),
                    PriceEpisodes = MapPriceEpisodes(dataLockEvent.EventId, matchedLearnerDataLockData)
                }).ToList()
            };
        }

        private static List<PriceEpisodeDto> MapPriceEpisodes(Guid learnerDataLockData, MatchedLearnerDataLockDataDto matchedLearnerDataLockData)
        {
            return matchedLearnerDataLockData
                .DataLockEventPriceEpisodes
                .Where(d => d.DataLockEventId == learnerDataLockData)
                .Select(priceEpisode => new PriceEpisodeDto
                {
                    Identifier = priceEpisode.PriceEpisodeIdentifier,
                    AgreedPrice = priceEpisode.AgreedPrice,
                    StartDate = priceEpisode.StartDate,
                    EndDate = priceEpisode.ActualEndDate,
                    NumberOfInstalments = priceEpisode.NumberOfInstalments,
                    InstalmentAmount = priceEpisode.InstalmentAmount,
                    CompletionAmount = priceEpisode.CompletionAmount,
                    Periods = MapPeriods(priceEpisode.PriceEpisodeIdentifier, matchedLearnerDataLockData),
                }).ToList();
        }

        private static List<PeriodDto> MapPeriods(string priceEpisodeIdentifier, MatchedLearnerDataLockDataDto matchedLearnerDataLockData)
        {
            var nonPayablePeriods = matchedLearnerDataLockData.DataLockEventNonPayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .Select(nonPayablePeriod =>
                {
                    var failures = matchedLearnerDataLockData.DataLockEventNonPayablePeriodFailures
                        .Where(f => f.DataLockEventNonPayablePeriodId ==
                                    nonPayablePeriod.DataLockEventNonPayablePeriodId).ToList();

                    var apprenticeship = 
                        matchedLearnerDataLockData.Apprenticeships.FirstOrDefault(a => a.Id == failures.First().ApprenticeshipId);

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

            var payablePeriods = matchedLearnerDataLockData.DataLockEventPayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .Select(payablePeriod =>
                {
                    var apprenticeship = matchedLearnerDataLockData.Apprenticeships.FirstOrDefault(a => a.Id == payablePeriod.ApprenticeshipId);

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
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.MatchedLearner.Application.Data;
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

            var firstEvent = matchedLearnerDataLockInfo.DataLockEvents.First();
            
            return new MatchedLearnerDto
            {
                StartDate = firstEvent.LearningStartDate.GetValueOrDefault(),
                EventTime = firstEvent.EventTime,
                IlrSubmissionDate = firstEvent.IlrSubmissionDateTime,
                IlrSubmissionWindowPeriod = firstEvent.CollectionPeriod,
                AcademicYear = firstEvent.AcademicYear,
                Ukprn = firstEvent.Ukprn,
                Uln = firstEvent.LearnerUln,
                Training = matchedLearnerDataLockInfo.DataLockEvents.Select(dataLockEvent => new TrainingDto
                {
                    Reference = dataLockEvent.LearningAimReference,
                    ProgrammeType = dataLockEvent.LearningAimProgrammeType,
                    StandardCode = dataLockEvent.LearningAimStandardCode,
                    FrameworkCode = dataLockEvent.LearningAimFrameworkCode,
                    PathwayCode = dataLockEvent.LearningAimPathwayCode,
                    FundingLineType = null,
                    StartDate = dataLockEvent.LearningStartDate.GetValueOrDefault(),
                    PriceEpisodes = MapPriceEpisodes(dataLockEvent.EventId, matchedLearnerDataLockInfo)
                }).ToList()
            };
        }

        private static List<PriceEpisodeDto> MapPriceEpisodes(Guid learnerDataLockData, MatchedLearnerDataLockInfo matchedLearnerDataLockInfo)
        {
            return matchedLearnerDataLockInfo
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
                    Periods = MapPeriods(priceEpisode.PriceEpisodeIdentifier, matchedLearnerDataLockInfo),
                }).ToList();
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

                    var apprenticeship = 
                        matchedLearnerDataLockInfo.Apprenticeships.FirstOrDefault(a => a.Id == failures.First().ApprenticeshipId);

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
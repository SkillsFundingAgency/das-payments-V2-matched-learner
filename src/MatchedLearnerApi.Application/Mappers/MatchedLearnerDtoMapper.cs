using System.Collections.Generic;
using System.Linq;
using MatchedLearnerApi.Application.Data.Models;
using MatchedLearnerApi.Types;

namespace MatchedLearnerApi.Application.Mappers
{
    public interface IMatchedLearnerDtoMapper
    {
        MatchedLearnerDto Map(List<DatalockEvent> datalockEvents);
    }

    public class MatchedLearnerDtoMapper : IMatchedLearnerDtoMapper
    {
        public MatchedLearnerDto Map(List<DatalockEvent> datalockEvents)
        {
            if (datalockEvents == null || !datalockEvents.Any())
                return null;

            var firstEvent = datalockEvents.First();
            
            return new MatchedLearnerDto
            {
                StartDate = firstEvent.LearningStartDate.GetValueOrDefault(),
                EventTime = firstEvent.EventTime,
                IlrSubmissionDate = firstEvent.IlrSubmissionDateTime,
                IlrSubmissionWindowPeriod = firstEvent.CollectionPeriod,
                AcademicYear = firstEvent.AcademicYear,
                Ukprn = firstEvent.Ukprn,
                Uln = firstEvent.LearnerUln,
                Training = datalockEvents.Select(dataLockEvent => new TrainingDto
                {
                    Reference = dataLockEvent.LearningAimReference,
                    ProgrammeType = dataLockEvent.LearningAimProgrammeType,
                    StandardCode = dataLockEvent.LearningAimStandardCode,
                    FrameworkCode = dataLockEvent.LearningAimFrameworkCode,
                    PathwayCode = dataLockEvent.LearningAimPathwayCode,
                    FundingLineType = null,
                    StartDate = dataLockEvent.LearningStartDate.GetValueOrDefault(),
                    //todo Status we think should be mapped on the price episode not on the Training
                    PriceEpisodes = MapPriceEpisodes(dataLockEvent)
                }).ToList()
            };
        }

        private static List<PriceEpisodeDto> MapPriceEpisodes(DatalockEvent dataLockEvent)
        {
            return dataLockEvent.PriceEpisodes.Select(priceEpisode => new PriceEpisodeDto
            {
                Identifier = priceEpisode.PriceEpisodeIdentifier,
                AgreedPrice = priceEpisode.AgreedPrice,
                StartDate = priceEpisode.StartDate,
                EndDate = priceEpisode.ActualEndDate,
                NumberOfInstalments = priceEpisode.NumberOfInstalments,
                InstalmentAmount = priceEpisode.InstalmentAmount,
                CompletionAmount = priceEpisode.CompletionAmount,
                Periods = MapPeriods(priceEpisode.PriceEpisodeIdentifier, dataLockEvent),
            }).ToList();
        }

        private static List<PeriodDto> MapPeriods(string priceEpisodeIdentifier, DatalockEvent datalockEvent)
        {
            var nonPayablePeriods = datalockEvent.NonPayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .SelectMany(nonPayablePeriod => nonPayablePeriod.Failures.GroupBy(failure => new PeriodDto
                {
                    Period = nonPayablePeriod.DeliveryPeriod,
                    IsPayable = false,
                    AccountId = failure.Apprenticeship?.AccountId ?? 0,
                    ApprenticeshipId = failure.ApprenticeshipId,
                    ApprenticeshipEmployerType = failure.Apprenticeship?.ApprenticeshipEmployerType ?? 0,
                    TransferSenderAccountId = failure.Apprenticeship?.TransferSendingEmployerAccountId ?? 0
                }).Select(group =>
                {
                    var period = group.Key;
                    period.DataLockFailures = group.Select(failure => (int)failure.DataLockFailureId).ToList();
                    return period;
                }));

            var payablePeriods = datalockEvent.PayablePeriods
                .Where(d => d.PriceEpisodeIdentifier == priceEpisodeIdentifier)
                .Select(payablePeriod => new PeriodDto
                {
                    Period = payablePeriod.DeliveryPeriod,
                    IsPayable = true,
                    AccountId = payablePeriod.Apprenticeship.AccountId,
                    ApprenticeshipId = payablePeriod.ApprenticeshipId,
                    ApprenticeshipEmployerType = payablePeriod.Apprenticeship.ApprenticeshipEmployerType,
                    TransferSenderAccountId = payablePeriod.Apprenticeship.TransferSendingEmployerAccountId
                });

            return payablePeriods.Union(nonPayablePeriods).ToList();
        }
    }
}
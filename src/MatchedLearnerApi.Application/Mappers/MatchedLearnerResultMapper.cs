using System.Collections.Generic;
using System.Linq;
using MatchedLearnerApi.Application.Models;
using MatchedLearnerApi.Types;

namespace MatchedLearnerApi.Application.Mappers
{
    public class MatchedLearnerResultMapper : IMatchedLearnerResultMapper
    {
        public MatchedLearnerResultDto Map(List<DatalockEvent> datalockEvents)
        {
            if (!datalockEvents.Any())
                return null;

            var firstEvent = datalockEvents.First();
            
            var groupedDatalockEvents = GroupDatalockEvents(datalockEvents);

            return new MatchedLearnerResultDto
            {
                StartDate = firstEvent.LearningStartDate.GetValueOrDefault(),
                EventTime = firstEvent.EventTime,
                IlrSubmissionDate = firstEvent.IlrSubmissionDateTime,
                IlrSubmissionWindowPeriod = firstEvent.CollectionPeriod,
                AcademicYear = firstEvent.AcademicYear,
                Ukprn = firstEvent.Ukprn,
                Uln = firstEvent.Uln,
                Training = groupedDatalockEvents.Select(dataLockEvent => new TrainingDto
                {
                    Reference = dataLockEvent.LearningAimReference,
                    ProgrammeType = dataLockEvent.ProgrammeType,
                    StandardCode = dataLockEvent.StandardCode,
                    FrameworkCode = dataLockEvent.FrameworkCode,
                    PathwayCode = dataLockEvent.PathwayCode,
                    FundingLineType = null,
                    StartDate = dataLockEvent.LearningStartDate.GetValueOrDefault(),
                    //todo Status we think should be mapped on the price episode not on the Training
                    PriceEpisodes = MapPriceEpisodes(dataLockEvent)
                }).ToList()
            };
        }

        public List<DatalockEvent> GroupDatalockEvents(IEnumerable<DatalockEvent> datalockEvents)
        {
            var result = datalockEvents.GroupBy(x => new
            {
                x.LearningAimReference,
                x.StandardCode,
                x.ProgrammeType,
                x.FrameworkCode,
                x.PathwayCode,
                x.AcademicYear,
                x.FundingLineType,
                x.LearningStartDate,
                x.Uln,
                x.Ukprn,
                x.CollectionPeriod,
                x.IlrSubmissionDateTime,
            }).Select(d => new DatalockEvent
            {
                NonPayablePeriods = d.SelectMany(p => p.NonPayablePeriods).ToList(),
                PayablePeriods = d.SelectMany(p => p.PayablePeriods).ToList(),
                PriceEpisodes = GroupPriceEpisodes(d.SelectMany(p => p.PriceEpisodes)),

                Ukprn = d.Key.Ukprn,
                Uln = d.Key.Uln,

                AcademicYear = d.Key.AcademicYear,
                CollectionPeriod = d.Key.CollectionPeriod,

                LearningAimReference = d.Key.LearningAimReference,

                PathwayCode = d.Key.PathwayCode,
                StandardCode = d.Key.StandardCode,
                FrameworkCode = d.Key.FrameworkCode,
                FundingLineType = d.Key.FundingLineType,
                ProgrammeType = d.Key.ProgrammeType,

                IlrSubmissionDateTime = d.Key.IlrSubmissionDateTime,
                LearningStartDate = d.Key.LearningStartDate,
            }).ToList();

            return result;
        }

        private static List<DatalockEventPriceEpisode> GroupPriceEpisodes(IEnumerable<DatalockEventPriceEpisode> priceEpisodes)
        {
            var result = priceEpisodes.GroupBy(pe => new
            {
                pe.PriceEpisodeIdentifier,
                pe.ActualEndDate,
                pe.Completed,
                pe.CompletionAmount,
                pe.InstalmentAmount,
                pe.NumberOfInstalments,
                pe.StartDate,
                pe.TotalNegotiatedPrice1,
                pe.TotalNegotiatedPrice2,
                pe.TotalNegotiatedPrice3,
                pe.TotalNegotiatedPrice4,
            }).Select(pe => new DatalockEventPriceEpisode
            {
                PriceEpisodeIdentifier = pe.Key.PriceEpisodeIdentifier,
                ActualEndDate = pe.Key.ActualEndDate,
                Completed = pe.Key.Completed,
                CompletionAmount = pe.Key.CompletionAmount,
                InstalmentAmount = pe.Key.InstalmentAmount,
                NumberOfInstalments = pe.Key.NumberOfInstalments,
                StartDate = pe.Key.StartDate,
                TotalNegotiatedPrice1 = pe.Key.TotalNegotiatedPrice1,
                TotalNegotiatedPrice2 = pe.Key.TotalNegotiatedPrice2,
                TotalNegotiatedPrice3 = pe.Key.TotalNegotiatedPrice3,
                TotalNegotiatedPrice4 = pe.Key.TotalNegotiatedPrice4,
            }).Distinct().ToList();

            return result;
        }

        private List<PriceEpisodeDto> MapPriceEpisodes(DatalockEvent dataLockEvent)
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

        private List<PeriodDto> MapPeriods(string priceEpisodeIdentifier, DatalockEvent datalockEvent)
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
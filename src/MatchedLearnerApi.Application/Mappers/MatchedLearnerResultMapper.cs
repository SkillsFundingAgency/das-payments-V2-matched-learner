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
            var orderedDatalockEvents = datalockEvents.OrderBy(x => x.LearningStartDate).ToList();
            var firstEvent = orderedDatalockEvents.First();

            foreach (var datalockEvent in datalockEvents)
            {
                if (datalockEvent.PriceEpisodes == null)
                    datalockEvent.PriceEpisodes = new List<DatalockEventPriceEpisode>();
                
                foreach (var datalockEventPriceEpisode in datalockEvent.PriceEpisodes)
                {
                    if (datalockEventPriceEpisode.NonPayablePeriods == null)
                        datalockEventPriceEpisode.NonPayablePeriods = new List<DatalockEventNonPayablePeriod>();

                    if (datalockEventPriceEpisode.PayablePeriods == null)
                        datalockEventPriceEpisode.PayablePeriods = new List<DatalockEventPayablePeriod>();

                    foreach (var datalockEventPayablePeriod in datalockEventPriceEpisode.PayablePeriods)
                    {
                        if (datalockEventPayablePeriod.Apprenticeship == null)
                            datalockEventPayablePeriod.Apprenticeship = new Apprenticeship();
                    }
                }
            }

            return new MatchedLearnerResultDto
            {
                StartDate = firstEvent.LearningStartDate.GetValueOrDefault(),
                EventTime = firstEvent.EventTime,
                IlrSubmissionDate = firstEvent.IlrSubmissionDateTime,
                IlrSubmissionWindowPeriod = firstEvent.IlrSubmissionWindowPeriod,
                AcademicYear = firstEvent.AcademicYear,
                Ukprn = firstEvent.Ukprn,
                Uln = firstEvent.Uln,
                Training = orderedDatalockEvents.Select(dataLockEvent => new TrainingDto
                {
                    Reference = dataLockEvent.Reference,
                    ProgrammeType = dataLockEvent.ProgrammeType,
                    StandardCode = dataLockEvent.StandardCode,
                    FrameworkCode = dataLockEvent.FrameworkCode,
                    PathwayCode = dataLockEvent.PathwayCode,
                    FundingLineType = dataLockEvent.FundingLineType,
                    StartDate = dataLockEvent.LearningStartDate.GetValueOrDefault(),
                    //todo Status we think should be mapped on the price episode not on the Training
                    PriceEpisodes = dataLockEvent.PriceEpisodes.Select(priceEpisode => new PriceEpisodeDto
                    {
                        Identifier = priceEpisode.Identifier,
                        AgreedPrice = priceEpisode.AgreedPrice,
                        StartDate = priceEpisode.StartDate,
                        EndDate = priceEpisode.ActualEndDate,
                        NumberOfInstalments = priceEpisode.NumberOfInstalments,
                        InstalmentAmount = priceEpisode.InstalmentAmount,
                        CompletionAmount = priceEpisode.CompletionAmount,
                        Periods = priceEpisode.PayablePeriods.Select(payablePeriod => new PeriodDto
                        {
                            Period = payablePeriod.Period,
                            IsPayable = true,
                            AccountId = payablePeriod.Apprenticeship.AccountId,
                            ApprenticeshipId = payablePeriod.ApprenticeshipId,
                            ApprenticeshipEmployerType = payablePeriod.Apprenticeship.ApprenticeshipEmployerType,
                            TransferSenderAccountId = payablePeriod.Apprenticeship.TransferSendingEmployerAccountId
                        }).Union(priceEpisode.NonPayablePeriods.SelectMany(nonPayablePeriod => nonPayablePeriod.Failures.GroupBy(failure => new PeriodDto
                        {
                            Period = nonPayablePeriod.Period,
                            IsPayable = false,
                            AccountId = failure.Apprenticeship.AccountId,
                            ApprenticeshipId = failure.ApprenticeshipId,
                            ApprenticeshipEmployerType = failure.Apprenticeship.ApprenticeshipEmployerType,
                            TransferSenderAccountId = failure.Apprenticeship.TransferSendingEmployerAccountId
                        }).Select(group =>
                        {
                            var period = group.Key;
                            period.DataLockFailures = group.Select(failure => (int)failure.DataLockFailureId).ToList();
                            return period;
                        }))).ToList()
                    }).ToList()
                }).ToList()
            };
        }
    }
}
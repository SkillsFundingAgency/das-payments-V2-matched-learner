using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchedLearnerApi.Application.Models;
using MatchedLearnerApi.Types;
using Microsoft.EntityFrameworkCore;

namespace MatchedLearnerApi.Application.Repositories
{
    public class EmployerIncentivesRepository : IEmployerIncentivesRepository
    {
        private readonly IPaymentsContext _context;
        private readonly IMatchedLearnerResultMapper _matchedLearnerResultMapper;

        public EmployerIncentivesRepository(IPaymentsContext context, IMatchedLearnerResultMapper matchedLearnerResultMapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _matchedLearnerResultMapper = matchedLearnerResultMapper ?? throw new ArgumentNullException(nameof(matchedLearnerResultMapper));
        }

        public async Task<MatchedLearnerResultDto> MatchedLearner(long ukprn, long uln)
        {
            var datalockEvent = await _context.DatalockEvents
                .Include(x => x.PriceEpisodes).ThenInclude(x => x.NonPayablePeriods).ThenInclude(x => x.Failures).ThenInclude(x => x.Apprenticeship)
                .Include(x => x.PriceEpisodes).ThenInclude(x => x.PayablePeriods).ThenInclude(x => x.Apprenticeship)
                .Where(x => x.Ukprn == ukprn && x.Uln == uln)
                .Where(x => x.AcademicYear == _context.DatalockEvents
                    .Where(y => y.Ukprn == ukprn && y.Uln == uln).OrderByDescending(y => y.AcademicYear).ThenByDescending(y => y.IlrSubmissionWindowPeriod).Select(y => y.AcademicYear).FirstOrDefault())
                .Where(x => x.IlrSubmissionWindowPeriod == _context.DatalockEvents
                    .Where(y => y.Ukprn == ukprn && y.Uln == uln).OrderByDescending(y => y.AcademicYear).ThenByDescending(y => y.IlrSubmissionWindowPeriod).Select(y => y.IlrSubmissionWindowPeriod).FirstOrDefault())
                .ToListAsync();

            return _matchedLearnerResultMapper.Map(datalockEvent);
        }
    }

    public interface IMatchedLearnerResultMapper
    {
        MatchedLearnerResultDto Map(List<DatalockEvent> datalockEvents);
    }

    public class MatchedLearnerResultMapper : IMatchedLearnerResultMapper
    {
        public MatchedLearnerResultDto Map(List<DatalockEvent> datalockEvents)
        {
            if (!datalockEvents.Any())
                return null;
            var orderedDatalockEvents = datalockEvents.OrderBy(x => x.LearningStartDate).ToList();
            var firstEvent = orderedDatalockEvents.First();

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
                        //todo end date - which to use? Actual or Planned or both?
                        NumberOfInstalments = priceEpisode.NumberOfInstalments,
                        InstalmentAmount = priceEpisode.InstalmentAmount,
                        CompletionAmount = priceEpisode.CompletionAmount,
                        Periods = priceEpisode.PayablePeriods.Select(payablePeriod => new PeriodDto
                        {
                            Period = payablePeriod.Period,
                            IsPayable = true,
                            AccountId = payablePeriod.Apprenticeship.AccountId,
                            ApprenticeshipId = payablePeriod.ApprenticeshipId,
                            ApprenticeshipEmployerType = payablePeriod.ApprenticeshipEmployerType,
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchedLearnerApi.Application.Mappers;
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

        public async Task<MatchedLearnerResultDto> GetMatchedLearnerResults(long ukprn, long uln)
        {
            var latestSuccessfulJob = await _context.LatestSuccessfulJobs
                .Where(y => y.Ukprn == ukprn)
                .OrderByDescending(y => y.AcademicYear)
                .ThenByDescending(y => y.CollectionPeriod)
                .FirstOrDefaultAsync();

            if (latestSuccessfulJob == null)
                return null;

            var transactionTypes = new List<byte> { 1, 2, 3 };

            var datalockEvents = await _context.DatalockEvents
                .Include(d => d.PriceEpisodes)
                .Include(d => d.PayablePeriods).ThenInclude(pp => pp.Apprenticeship)
                .Include(d => d.NonPayablePeriods).ThenInclude(npp => npp.Failures).ThenInclude(f => f.Apprenticeship)
                .Where(x => x.LearningAimReference == "ZPROG001")
                .Where(x => x.Ukprn == ukprn && x.Uln == uln)
                .Where(x => x.Uln == uln)
                .Where(x => x.JobId == latestSuccessfulJob.DcJobId)
                .Where(x => x.AcademicYear == latestSuccessfulJob.AcademicYear)
                .Where(x => x.CollectionPeriod == latestSuccessfulJob.CollectionPeriod)
                .OrderBy(x => x.LearningStartDate)
                .Select(d => new DatalockEvent
                {
                    NonPayablePeriods = d.NonPayablePeriods.Where(npp => transactionTypes.Contains(npp.TransactionType) && npp.PriceEpisodeIdentifier != null).ToList(),
                    PayablePeriods = d.PayablePeriods.Where(pp => transactionTypes.Contains(pp.TransactionType) && pp.PriceEpisodeIdentifier != null).ToList(),
                    PriceEpisodes = d.PriceEpisodes,

                    Ukprn = d.Ukprn,
                    Uln = d.Uln,

                    AcademicYear = d.AcademicYear,
                    CollectionPeriod = d.CollectionPeriod,

                    LearningAimReference = d.LearningAimReference,

                    PathwayCode = d.PathwayCode,
                    StandardCode = d.StandardCode,
                    FrameworkCode = d.FrameworkCode,
                    FundingLineType = d.FundingLineType,
                    ProgrammeType = d.ProgrammeType,

                    IlrSubmissionDateTime = d.IlrSubmissionDateTime,
                    LearningStartDate = d.LearningStartDate,
                })
                .ToListAsync();

            return _matchedLearnerResultMapper.Map(datalockEvents);
        }
    }
}

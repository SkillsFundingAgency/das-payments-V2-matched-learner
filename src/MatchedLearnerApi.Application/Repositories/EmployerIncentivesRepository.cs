using System;
using System.Linq;
using System.Threading.Tasks;
using MatchedLearnerApi.Application.Mappers;
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
            var latestPeriod = await _context.DatalockEvents
                .Where(y => y.Ukprn == ukprn && y.Uln == uln)
                .OrderByDescending(y => y.AcademicYear)
                .ThenByDescending(y => y.IlrSubmissionWindowPeriod)
                .FirstOrDefaultAsync();

            if (latestPeriod == null)
                return null;

            var datalockEvent = await _context.DatalockEvents
                .Include(x => x.PriceEpisodes).ThenInclude(x => x.NonPayablePeriods).ThenInclude(x => x.Failures).ThenInclude(x => x.Apprenticeship)
                .Include(x => x.PriceEpisodes).ThenInclude(x => x.PayablePeriods).ThenInclude(x => x.Apprenticeship)
                .Where(x => x.Ukprn == ukprn && x.Uln == uln)
                .Where(x => x.AcademicYear == latestPeriod.AcademicYear)
                .Where(x => x.IlrSubmissionWindowPeriod == latestPeriod.IlrSubmissionWindowPeriod)
                .ToListAsync();

            return _matchedLearnerResultMapper.Map(datalockEvent);
        }
    }
}

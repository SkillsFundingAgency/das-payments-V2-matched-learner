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
                .Where(x => x.Ukprn == ukprn && x.Learner.Uln == uln)
                .GroupBy(x => new {x.AcademicYear, x.IlrSubmissionWindowPeriod})
                .OrderByDescending(x => x.Key.AcademicYear)
                .ThenByDescending(x => x.Key.IlrSubmissionWindowPeriod)
                .FirstOrDefaultAsync();

            return _matchedLearnerResultMapper.Map(datalockEvent);
        }
    }

    public interface IMatchedLearnerResultMapper
    {
        MatchedLearnerResultDto Map(IEnumerable<DatalockEvent> datalockEvents);
    }

    public class MatchedLearnerResultMapper : IMatchedLearnerResultMapper
    {
        public MatchedLearnerResultDto Map(IEnumerable<DatalockEvent> datalockEvents)
        {
            throw new System.NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchedLearnerApi.Application.Repositories
{
    public class EmployerIncentivesRepository
    {
        private readonly PaymentsContext _context;

        public EmployerIncentivesRepository(PaymentsContext context)
        {
            _context = context;
        }

        public Task<MatchedLearnerResult> MatchedLearner(long ukprn, long uln)
        {
            return _context.DatalockEvents
                .Where(x => x.Ukprn == ukprn && x.Learner.Uln == uln)
                .Select(x => new MatchedLearnerResult())
                .FirstOrDefaultAsync();
        }
    }

    public class MatchedLearnerResult
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public Guid EventId { get; set; }
        public DateTimeOffset IlrSubmissionDate { get; set; }
        public int IlrSubmissionWindowPeriod { get; set; }
        public int AcademicYear { get; set; }
        public long Ukprn { get; set; }
        public Learner Learner { get; set; }
        public List<DatalockEvent> Training { get; set; } = new List<DatalockEvent>();
    }
}

using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public class MatchedLearnerResultDto
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public Guid EventId { get; set; }
        public DateTimeOffset IlrSubmissionDate { get; set; }
        public int IlrSubmissionWindowPeriod { get; set; }
        public int AcademicYear { get; set; }
        public long Ukprn { get; set; }
        public LearnerDto Learner { get; set; }
        public List<DatalockEventDto> Training { get; set; } = new List<DatalockEventDto>();
    }
}

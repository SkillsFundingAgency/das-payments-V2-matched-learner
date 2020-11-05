using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Types
{
    public class MatchedLearnerDto
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public DateTimeOffset IlrSubmissionDate { get; set; }
        public int IlrSubmissionWindowPeriod { get; set; }
        public int AcademicYear { get; set; }
        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public List<TrainingDto> Training { get; set; } = new List<TrainingDto>();
    }
}

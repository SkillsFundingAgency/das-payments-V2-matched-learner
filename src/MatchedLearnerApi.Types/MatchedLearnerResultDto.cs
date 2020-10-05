﻿using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public class MatchedLearnerResultDto
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

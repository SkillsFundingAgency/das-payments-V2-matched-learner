using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class ImportMatchedLearnerData
    {
        public byte CollectionPeriod { get; set; }
        public long JobId { get; set; }
        public long Ukprn { get; set; }
        public short AcademicYear { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public DateTimeOffset EventTime { get; set; }
    }
}
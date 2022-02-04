using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class SubmissionJobModel
    {
        public long Id { get; set; }
        public long DcJobId { get; set; }
        public long Ukprn { get; set; }
        public byte CollectionPeriod { get; set; }
        public short AcademicYear { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public DateTimeOffset EventTime { get; set; }
    }
}
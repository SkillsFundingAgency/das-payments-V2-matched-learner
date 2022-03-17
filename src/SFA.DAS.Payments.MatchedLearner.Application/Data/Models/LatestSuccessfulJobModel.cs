using System;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.Models
{
    public class LatestSuccessfulJobModel
    {
        public long JobId { get; set; }
        public long Ukprn { get; set; }
        public long DcJobId { get; set; }
        public short AcademicYear { get; set; }
        public byte CollectionPeriod { get; set; }
        public DateTime IlrSubmissionTime { get; set; }
    }
}
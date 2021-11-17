using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class TrainingModel
    {
        public long Id { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public Guid EventId { get; set; }
        public DateTime IlrSubmissionDate { get; set; }
        public byte IlrSubmissionWindowPeriod { get; set; }
        public short AcademicYear { get; set; }
        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public string Reference { get; set; }
        public int ProgrammeType { get; set; }
        public int StandardCode { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
        public string FundingLineType { get; set; }
        public DateTime StartDate { get; set; }
        public int CompletionStatus { get; set; }

        public List<PriceEpisodeModel> PriceEpisodes { get; set; } = new List<PriceEpisodeModel>();
    }
}

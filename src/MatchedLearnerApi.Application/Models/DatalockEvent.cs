using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Application.Models
{
    public class DatalockEvent
    {
        public long Id { get; set; }
        public long JobId { get; set; }

        // "Header" info
        public Guid EventId { get; set; }
        public short AcademicYear { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public byte CollectionPeriod { get; set; }
        public long Ukprn { get; set; }
        public long Uln { get; set; }


        // "Training" info
        public string LearningAimReference { get; set; }

        public int ProgrammeType { get; set; }
        public int StandardCode { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
        public string FundingLineType { get; set; }
        
        public DateTime? LearningStartDate { get; set; }

        public virtual List<DatalockEventPriceEpisode> PriceEpisodes { get; set; } = new List<DatalockEventPriceEpisode>();
        public virtual List<DatalockEventNonPayablePeriod> NonPayablePeriods { get; set; } = new List<DatalockEventNonPayablePeriod>();
        public virtual List<DatalockEventPayablePeriod> PayablePeriods { get; set; } = new List<DatalockEventPayablePeriod>();
    }
}

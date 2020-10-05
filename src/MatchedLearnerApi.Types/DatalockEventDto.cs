using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public class DatalockEventDto
    {
        public long Id { get; set; }

        // "Header" info
        public Guid EventId { get; set; }
        public short AcademicYear { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public int IlrSubmissionWindowPeriod { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long Ukprn { get; set; }


        public LearnerDto Learner { get; set; }


        // "Training" info
        public string Reference { get; set; }
        public int ProgrammeType { get; set; }
        public int StandardCode { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
        public string FundingLineType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public byte CompletionStatus { get; set; }


        public List<DatalockEventPriceEpisodeDto> PriceEpisodes { get; set; } = new List<DatalockEventPriceEpisodeDto>();
        


        






        public decimal CompletionAmount { get; set; }
        public decimal InstalmentAmount { get; set; }
        public short NumberOfInstalments { get; set; }
        public DateTime? LearningStartDate { get; set; }
        
        
        
        public ContractTypeDto ContractTypeDto { get; set; }
        public string AgreementId { get; set; }
        public long? LearningAimSequenceNumber { get; set; }
        public bool IsPayable { get; set; }
    }
}

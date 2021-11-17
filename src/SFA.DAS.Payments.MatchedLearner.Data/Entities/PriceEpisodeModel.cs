using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class PriceEpisodeModel
    {
        public long Id { get; set; }
        public long TrainingId { get; set; }
        public string Identifier { get; set; }
        public short AcademicYear { get; set; }
        public byte CollectionPeriod { get; set; }
        public decimal AgreedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int NumberOfInstalments { get; set; }
        public decimal InstalmentAmount { get; set; }
        public decimal CompletionAmount { get; set; }
        public DateTime? TotalNegotiatedPriceStartDate { get; set; }
        public virtual List<PeriodModel> Periods { get; set; } = new List<PeriodModel>();
    }
}

using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Types
{
    public class PriceEpisodeDto
    {
        public string Identifier { get; set; }
        public decimal AgreedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfInstalments { get; set; }
        public decimal InstalmentAmount { get; set; }
        public decimal CompletionAmount { get; set; }
        public virtual List<PeriodDto> Periods { get; set; } = new List<PeriodDto>();
    }
}

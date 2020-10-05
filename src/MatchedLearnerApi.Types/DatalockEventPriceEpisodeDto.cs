using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public class DatalockEventPriceEpisodeDto
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }

        public string Identifier { get; set; }

        public decimal AgreedPrice
        {
            get
            {
                if (TotalNegotiatedPrice3 != 0)
                    return TotalNegotiatedPrice3 + TotalNegotiatedPrice4;
                return TotalNegotiatedPrice1 + TotalNegotiatedPrice2;
            }
        }

        public decimal TotalNegotiatedPrice1 { get; set; }
        public decimal TotalNegotiatedPrice2 { get; set; }
        public decimal TotalNegotiatedPrice3 { get; set; }
        public decimal TotalNegotiatedPrice4 { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        
        public int NumberOfInstalments { get; set; }
        public decimal InstalmentAmount { get; set; }
        public decimal CompletionAmount { get; set; }

        public virtual List<DatalockEventNonPayablePeriodDto> NonPayablePeriods { get; set; } = new List<DatalockEventNonPayablePeriodDto>();
        public virtual List<DatalockEventPayablePeriodDto> PayablePeriods { get; set; } = new List<DatalockEventPayablePeriodDto>();


        public bool Completed { get; set; }
    }
}

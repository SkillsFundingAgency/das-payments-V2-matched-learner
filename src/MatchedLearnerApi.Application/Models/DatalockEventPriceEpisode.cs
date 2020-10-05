using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Application.Models
{
    public class DatalockEventPriceEpisode
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

        public virtual List<DatalockEventNonPayablePeriod> NonPayablePeriods { get; set; } = new List<DatalockEventNonPayablePeriod>();
        public virtual List<DatalockEventPayablePeriod> PayablePeriods { get; set; } = new List<DatalockEventPayablePeriod>();


        public bool Completed { get; set; }
    }
}

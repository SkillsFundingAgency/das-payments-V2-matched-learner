using System;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.Models
{
    public class DatalockEventPriceEpisode
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

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
        public DateTime? ActualEndDate { get; set; }

        public int NumberOfInstalments { get; set; }
        public decimal InstalmentAmount { get; set; }
        public decimal CompletionAmount { get; set; }

        public bool Completed { get; set; }
    }
}

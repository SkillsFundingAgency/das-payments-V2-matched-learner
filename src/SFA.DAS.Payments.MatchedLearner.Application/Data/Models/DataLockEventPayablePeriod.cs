using System;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.Models
{
    public class DataLockEventPayablePeriod
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public byte TransactionType { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
    
        public long? ApprenticeshipId { get; set; }
    }
}

using System;

namespace MatchedLearnerApi.Application.Data.Models
{
    public class DatalockEventPayablePeriod
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public byte TransactionType { get; set; }
        public byte DeliveryPeriod { get; set; }
    
        public long? ApprenticeshipId { get; set; }
        
        public virtual Apprenticeship Apprenticeship { get; set; }
    }
}

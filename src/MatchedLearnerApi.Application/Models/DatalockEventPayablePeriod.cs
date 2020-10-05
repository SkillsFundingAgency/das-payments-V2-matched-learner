using System;

namespace MatchedLearnerApi.Application.Models
{
    public class DatalockEventPayablePeriod
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public long? ApprenticeshipId { get; set; }

        public byte Period { get; set; }
        public bool IsPayable => true;
        public byte ApprenticeshipEmployerType { get; set; }
        public virtual Apprenticeship Apprenticeship { get; set; }
    }
}

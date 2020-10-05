using System;

namespace MatchedLearnerApi.Types
{
    public class DatalockEventPayablePeriodDto
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public long? ApprenticeshipId { get; set; }

        public int Period { get; set; }
        public bool IsPayable => true;
        public ApprenticeshipEmployerTypeDto? ApprenticeshipEmployerType { get; set; }
        public virtual ApprenticeshipDto Apprenticeship { get; set; }
    }
}

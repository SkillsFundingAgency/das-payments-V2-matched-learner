using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public class DatalockEventNonPayablePeriodDto
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        
        public byte Period { get; set; }
        public virtual List<DatalockEventNonPayablePeriodFailureDto> Failures { get; set; } = new List<DatalockEventNonPayablePeriodFailureDto>();
    }
}

using System;

namespace MatchedLearnerApi.Types
{
    public class DatalockEventNonPayablePeriodFailureDto
    {
        public long Id { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public long? ApprenticeshipId { get; set; }

        public DatalockErrorCodeDto DataLockFailure { get; set; }
        public virtual ApprenticeshipDto Apprenticeship { get; set; }
    }
}

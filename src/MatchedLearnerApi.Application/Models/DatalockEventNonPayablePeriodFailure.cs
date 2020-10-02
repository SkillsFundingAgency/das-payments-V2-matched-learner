using System;

namespace MatchedLearnerApi.Application.Models
{
    public class DatalockEventNonPayablePeriodFailure
    {
        public long Id { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public long? ApprenticeshipId { get; set; }

        public DatalockErrorCode DataLockFailure { get; set; }
        public virtual Apprenticeship Apprenticeship { get; set; }
    }
}

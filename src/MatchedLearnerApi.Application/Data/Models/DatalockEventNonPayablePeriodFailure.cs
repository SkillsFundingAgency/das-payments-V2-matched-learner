using System;

namespace MatchedLearnerApi.Application.Data.Models
{
    public class DatalockEventNonPayablePeriodFailure
    {
        public long Id { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public long? ApprenticeshipId { get; set; }

        public byte DataLockFailureId { get; set; }
        public virtual Apprenticeship Apprenticeship { get; set; }
    }
}

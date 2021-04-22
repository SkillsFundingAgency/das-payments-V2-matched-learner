using System;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.Models
{
    public class DataLockEventNonPayablePeriodFailure
    {
        public long Id { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public long? ApprenticeshipId { get; set; }
        public byte DataLockFailureId { get; set; }
    }
}

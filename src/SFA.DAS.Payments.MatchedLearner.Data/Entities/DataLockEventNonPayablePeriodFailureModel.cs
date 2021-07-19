using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class DataLockEventNonPayablePeriodFailureModel
    {
        public long Id { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public byte DataLockFailureId { get; set; }
        public long? ApprenticeshipId { get; set; }
    }
}
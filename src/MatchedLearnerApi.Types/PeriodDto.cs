using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public struct PeriodDto
    {
        public int Period { get; set; }
        public bool IsPayable { get; set; }
        public long AccountId { get; set; }
        public long? ApprenticeshipId { get; set; }
        public int ApprenticeshipEmployerType { get; set; }
        public long TransferSenderAccountId { get; set; }
        public List<int> DataLockFailures { get; set; }
    }
}

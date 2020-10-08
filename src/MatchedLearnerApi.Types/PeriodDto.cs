using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public struct PeriodDto : IEquatable<PeriodDto>
    {
        public bool Equals(PeriodDto other)
        {
            return Period == other.Period && 
                   IsPayable == other.IsPayable && 
                   AccountId == other.AccountId && 
                   ApprenticeshipId == other.ApprenticeshipId && 
                   ApprenticeshipEmployerType == other.ApprenticeshipEmployerType && 
                   TransferSenderAccountId == other.TransferSenderAccountId;
        }

        public override bool Equals(object obj)
        {
            return obj is PeriodDto other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Period;
                hashCode = (hashCode * 397) ^ IsPayable.GetHashCode();
                hashCode = (hashCode * 397) ^ AccountId.GetHashCode();
                hashCode = (hashCode * 397) ^ ApprenticeshipId.GetHashCode();
                hashCode = (hashCode * 397) ^ ApprenticeshipEmployerType;
                hashCode = (hashCode * 397) ^ TransferSenderAccountId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PeriodDto left, PeriodDto right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PeriodDto left, PeriodDto right)
        {
            return !left.Equals(right);
        }

        public int Period { get; set; }
        public bool IsPayable { get; set; }
        public long AccountId { get; set; }
        public long? ApprenticeshipId { get; set; }
        public int ApprenticeshipEmployerType { get; set; }
        public long TransferSenderAccountId { get; set; }
        public List<int> DataLockFailures { get; set; }
    }
}

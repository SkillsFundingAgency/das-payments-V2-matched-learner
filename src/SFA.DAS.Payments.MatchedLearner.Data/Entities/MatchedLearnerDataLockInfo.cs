using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class MatchedLearnerDataLockInfo
    {
        public List<DataLockEventModel> DataLockEvents { get; set; } = new List<DataLockEventModel>();
        public List<DataLockEventPriceEpisodeModel> DataLockEventPriceEpisodes { get; set; } = new List<DataLockEventPriceEpisodeModel>();
        public List<DataLockEventPayablePeriodModel> DataLockEventPayablePeriods { get; set; } = new List<DataLockEventPayablePeriodModel>();
        public List<DataLockEventNonPayablePeriodModel> DataLockEventNonPayablePeriods { get; set; } = new List<DataLockEventNonPayablePeriodModel>();
        public List<DataLockEventNonPayablePeriodFailureModel> DataLockEventNonPayablePeriodFailures { get; set; } = new List<DataLockEventNonPayablePeriodFailureModel>();
        public List<ApprenticeshipModel> Apprenticeships { get; set; } = new List<ApprenticeshipModel>();
    }
}
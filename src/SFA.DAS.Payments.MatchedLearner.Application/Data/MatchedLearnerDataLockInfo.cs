using System.Collections.Generic;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data
{
    public class MatchedLearnerDataLockInfo
    {
        public List<LatestSuccessfulJobModel> LatestSuccessfulJobs { get; set; } = new List<LatestSuccessfulJobModel>();
        public List<DataLockEvent> DataLockEvents { get; set; } = new List<DataLockEvent>();
        public List<DataLockEventPriceEpisode> DataLockEventPriceEpisodes { get; set; } = new List<DataLockEventPriceEpisode>();
        public List<DataLockEventPayablePeriod> DataLockEventPayablePeriods { get; set; } = new List<DataLockEventPayablePeriod>();
        public List<DataLockEventNonPayablePeriod> DataLockEventNonPayablePeriods { get; set; } = new List<DataLockEventNonPayablePeriod>();
        public List<DataLockEventNonPayablePeriodFailure> DataLockEventNonPayablePeriodFailures { get; set; } = new List<DataLockEventNonPayablePeriodFailure>();
        public List<Apprenticeship> Apprenticeships { get; set; } = new List<Apprenticeship>();
    }
}

using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class DataLockEventModel : PaymentsEventModel
    {
        public long Id { get; set; }
        public Guid EarningEventId { get; set; }
        public ContractType ContractType { get; set; }
        public string AgreementId { get; set; }
        public List<DataLockEventPriceEpisodeModel> PriceEpisodes { get; set; }
        public virtual List<DataLockEventNonPayablePeriodModel> NonPayablePeriods { get; set; } = new List<DataLockEventNonPayablePeriodModel>();
        public virtual List<DataLockEventPayablePeriodModel> PayablePeriods { get; set; } = new List<DataLockEventPayablePeriodModel>();
        public string IlrFileName { get; set; }
        public decimal? SfaContributionPercentage { get; set; }
        public string EventType { get; set; }
        public bool IsPayable { get; set; }
        public DataLockSource DataLockSource { get; set; }
    }
}
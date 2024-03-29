﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class DataLockEventNonPayablePeriodModel
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public byte TransactionType { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
        public decimal? SfaContributionPercentage { get; set; }
        public DateTime? CensusDate { get; set; }
        public DateTime? LearningStartDate { get; set; }
        public virtual List<DataLockEventNonPayablePeriodFailureModel> Failures { get; set; } = new List<DataLockEventNonPayablePeriodFailureModel>();
    }
}
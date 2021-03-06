﻿using System;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.Models
{
    public class DataLockEventNonPayablePeriod
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public byte TransactionType { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }

        public Guid DataLockEventNonPayablePeriodId { get; set; }
    }
}

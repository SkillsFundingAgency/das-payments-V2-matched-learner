using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class DataLockEventPayablePeriodModel
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public byte TransactionType { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
        public decimal? SfaContributionPercentage { get; set; }
        public DateTime? LearningStartDate { get; set; }
        public long? ApprenticeshipId { get; set; }
        public long? ApprenticeshipPriceEpisodeId { get; set; }
        public ApprenticeshipEmployerType? ApprenticeshipEmployerType { get; set; }
    }
}
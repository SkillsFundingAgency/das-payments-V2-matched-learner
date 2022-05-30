using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class MigrationRunAttemptModel
    {
        public long Id { get; set; }
        public Guid? MigrationRunId { get; set; }
        public long? Ukprn { get; set; }
        public MigrationStatus? Status { get; set; }
        public int TrainingCount { get; set; }
        public DateTimeOffset? CompletionTime { get; set; }
        public int? BatchNumber { get; set; }
        public int? TotalBatches { get; set; }
        public string BatchUlns { get; set; }
    }
}

using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class MigrationRunAttemptModel
    {
        public long Id { get; set; }
        public Guid? MigrationRunId { get; set; }
        public long? Ukprn { get; set; }
        public MigrationStatus? Status { get; set; }
        public int LearnerCount { get; set; }
        public DateTimeOffset? CompletionTime { get; set; }
    }
}

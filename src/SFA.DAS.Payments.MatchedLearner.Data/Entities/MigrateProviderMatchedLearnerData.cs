using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class MigrateProviderMatchedLearnerData
    {
        public Guid MigrationRunId { get; set; }
        public long Ukprn { get; set; }
        public TrainingModel[] TrainingData { get; set; }
        public int? BatchNumber { get; set; }
        public int? TotalBatches { get; set; }
        public bool IsFirstBatch => TrainingData == null;
    }
}

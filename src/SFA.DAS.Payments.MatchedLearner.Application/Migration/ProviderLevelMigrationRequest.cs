using System;
using System.Collections.Generic;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public class ProviderLevelMigrationRequest : ICommand
    {
        public Guid MigrationRunId { get; set; }
        public long Ukprn { get; set; }
        public TrainingModel[] TrainingData { get; set; }
        public int? BatchNumber { get; set; }
        public int? TotalBatches { get; set; }
        public bool IsFirstBatch => TrainingData == null;
    }
}

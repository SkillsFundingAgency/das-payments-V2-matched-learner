using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class StartProviderMatchedLearnerDataMigration
    {
        public Guid RunId { get; set; } = Guid.NewGuid();
    }
}
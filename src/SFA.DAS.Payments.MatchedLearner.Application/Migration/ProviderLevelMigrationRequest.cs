using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public class ProviderLevelMigrationRequest
    {
        public Guid MigrationRunId { get; set; }
        public long Ukprn { get; set; }
    }
}

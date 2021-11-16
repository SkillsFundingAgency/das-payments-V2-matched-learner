using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public class ProviderLevelMigrationRequest : ICommand
    {
        public Guid MigrationRunId { get; set; }
        public long Ukprn { get; set; }
    }
}

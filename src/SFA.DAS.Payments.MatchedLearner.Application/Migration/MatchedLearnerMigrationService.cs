using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IMatchedLearnerMigrationService
    {
        Task TriggerMigrationForAllProviders();
    }
    public class MatchedLearnerMigrationService : IMatchedLearnerMigrationService
    {
        private readonly MatchedLearnerDataContext _matchedLearnerDataContext;
        private readonly IEndpointInstanceFactory _endpointInstanceFactory;
        private readonly string _providerLevelMatchedLearnerMigration;

        public MatchedLearnerMigrationService(MatchedLearnerDataContext matchedLearnerDataContext, IEndpointInstanceFactory endpointInstanceFactory, string providerLevelMatchedLearnerMigration)
        {
            _matchedLearnerDataContext = matchedLearnerDataContext;
            _endpointInstanceFactory = endpointInstanceFactory;
            _providerLevelMatchedLearnerMigration = providerLevelMatchedLearnerMigration;
        }

        public async Task TriggerMigrationForAllProviders()
        {
            var migrationRunId = Guid.NewGuid();
            var providers = _matchedLearnerDataContext.DataLockEvent.Select(x => x.Ukprn).Distinct();

            var endpointInstance = await _endpointInstanceFactory.GetEndpointInstance();

            foreach (var provider in providers)
            {
                //await endpointInstance.Send(new ProviderLevelMigrationRequest{ MigrationRunId = migrationRunId, Ukprn = provider }, new SendOptions(). ).ConfigureAwait(false); todo fix this
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

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
        private readonly IProviderMigrationRepository _providerMigrationRepository;

        public MatchedLearnerMigrationService(MatchedLearnerDataContext matchedLearnerDataContext, IEndpointInstanceFactory endpointInstanceFactory, string providerLevelMatchedLearnerMigration, IProviderMigrationRepository providerMigrationRepository)
        {
            _matchedLearnerDataContext = matchedLearnerDataContext;
            _endpointInstanceFactory = endpointInstanceFactory;
            _providerLevelMatchedLearnerMigration = providerLevelMatchedLearnerMigration;
            _providerMigrationRepository = providerMigrationRepository;
        }

        public async Task TriggerMigrationForAllProviders()
        {
            var migrationRunId = Guid.NewGuid();
            var providers = await _matchedLearnerDataContext.DataLockEvent.Select(x => x.Ukprn).Distinct().ToListAsync();

            var endpointInstance = await _endpointInstanceFactory.GetEndpointInstance();

            foreach (var provider in providers)
            {
                if(await IsProviderAlreadyProcessed(provider))
                    continue;
                
                var options = new SendOptions();
                options.SetDestination(_providerLevelMatchedLearnerMigration);
                await endpointInstance.Send(new ProviderLevelMigrationRequest{ MigrationRunId = migrationRunId, Ukprn = provider }, options);
            }
        }

        private async Task<bool> IsProviderAlreadyProcessed(long ukprn)
        {
            var existingAttempts = await _providerMigrationRepository.GetProviderMigrationAttempts(ukprn);
            if (existingAttempts.Any(x => x.Status == MigrationStatus.Completed && x.BatchNumber == null))
                return true;

            return existingAttempts
                .GroupBy(x => x.MigrationRunId)
                .Any(run => run.Where(x => x.BatchNumber != null).All(x => x.Status == MigrationStatus.Completed));
        }
    }
}

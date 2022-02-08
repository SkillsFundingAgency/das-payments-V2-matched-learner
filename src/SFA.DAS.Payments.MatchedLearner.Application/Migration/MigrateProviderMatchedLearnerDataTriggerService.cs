using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Functions.Migration;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IMigrateProviderMatchedLearnerDataTriggerService
    {
        Task TriggerMigration(IMessageHandlerContext messageHandlerContext);
    }

    public class MigrateProviderMatchedLearnerDataTriggerService : IMigrateProviderMatchedLearnerDataTriggerService
    {
        private readonly MatchedLearnerDataContext _matchedLearnerDataContext;
        private readonly ILogger<MigrateProviderMatchedLearnerDataTriggerService> _logger;
        private readonly IProviderMigrationRepository _providerMigrationRepository;

        public MigrateProviderMatchedLearnerDataTriggerService(MatchedLearnerDataContext matchedLearnerDataContext, IProviderMigrationRepository providerMigrationRepository, ILogger<MigrateProviderMatchedLearnerDataTriggerService> logger)
        {
            _matchedLearnerDataContext = matchedLearnerDataContext;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _providerMigrationRepository = providerMigrationRepository;
        }

        public async Task TriggerMigration(IMessageHandlerContext messageHandlerContext)
        {

            var migrationRunId = Guid.NewGuid();
            var providers = await _matchedLearnerDataContext.DataLockEvent.Select(x => x.Ukprn).Distinct().ToListAsync();

            _logger.LogInformation($"Staring Data Migration for {providers.Count} providers");

            foreach (var provider in providers)
            {
                if (await IsProviderAlreadyProcessed(provider))
                {
                    _logger.LogWarning($"Provider with Ukprn {provider} was already migrated");
                    continue;
                }

                _logger.LogInformation($"Staring Data Migration for providers Ukprn: {provider}");

                await messageHandlerContext.Send(new MigrateProviderMatchedLearnerData
                {
                    MigrationRunId = migrationRunId,
                    Ukprn = provider
                }, SendLocally.Options);
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
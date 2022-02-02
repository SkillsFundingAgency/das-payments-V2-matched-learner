using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IMigrateProviderMatchedLearnerDataTriggerService
    {
        Task TriggerMigration();
    }

    public class MigrateProviderMatchedLearnerDataTriggerService : IMigrateProviderMatchedLearnerDataTriggerService
    {
        private readonly MatchedLearnerDataContext _matchedLearnerDataContext;
        private readonly IEndpointInstance _endpointInstance;
        private readonly ILogger<MigrateProviderMatchedLearnerDataTriggerService> _logger;
        private readonly IProviderMigrationRepository _providerMigrationRepository;
        private readonly ApplicationSettings _applicationSettings;

        public MigrateProviderMatchedLearnerDataTriggerService(ApplicationSettings applicationSettings, IEndpointInstance endpointInstance, MatchedLearnerDataContext matchedLearnerDataContext, IProviderMigrationRepository providerMigrationRepository, ILogger<MigrateProviderMatchedLearnerDataTriggerService> logger)
        {
            _matchedLearnerDataContext = matchedLearnerDataContext;
            _endpointInstance = endpointInstance;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _providerMigrationRepository = providerMigrationRepository;
            _applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
        }

        public async Task TriggerMigration()
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

                var options = new SendOptions();
                options.SetDestination(_applicationSettings.MigrationQueue);
                await _endpointInstance.Send(new MigrateProviderMatchedLearnerData { MigrationRunId = migrationRunId, Ukprn = provider }, options);
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
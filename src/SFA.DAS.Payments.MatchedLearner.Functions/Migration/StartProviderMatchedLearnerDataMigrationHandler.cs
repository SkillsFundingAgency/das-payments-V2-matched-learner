using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public class StartProviderMatchedLearnerDataMigrationHandler : IHandleMessages<StartProviderMatchedLearnerDataMigration>
    {
        private readonly IMigrateProviderMatchedLearnerDataTriggerService _migrateProviderMatchedLearnerDataTriggerService;
        private readonly ILogger<StartProviderMatchedLearnerDataMigrationHandler> _logger;

        public StartProviderMatchedLearnerDataMigrationHandler(IMigrateProviderMatchedLearnerDataTriggerService migrateProviderMatchedLearnerDataTriggerService, ILogger<StartProviderMatchedLearnerDataMigrationHandler> logger)
        {
            _migrateProviderMatchedLearnerDataTriggerService = migrateProviderMatchedLearnerDataTriggerService ?? throw new ArgumentNullException(nameof(migrateProviderMatchedLearnerDataTriggerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(StartProviderMatchedLearnerDataMigration message, IMessageHandlerContext context)
        {
            try
            {
                await _migrateProviderMatchedLearnerDataTriggerService.TriggerMigration(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error Starting Provider MatchedLearner Data Migration, Please see internal exception for more info {exception}");
                throw;
            }
        }
    }
}
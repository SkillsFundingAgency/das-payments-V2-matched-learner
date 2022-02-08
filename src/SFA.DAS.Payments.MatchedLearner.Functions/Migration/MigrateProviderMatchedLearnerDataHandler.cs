using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public class MigrateProviderMatchedLearnerDataHandler : IHandleMessages<MigrateProviderMatchedLearnerData>
    {
        private readonly IMigrateProviderMatchedLearnerDataService _migrateProviderMatchedLearnerDataService;
        private readonly ILogger<MigrateProviderMatchedLearnerDataHandler> _logger;

        public MigrateProviderMatchedLearnerDataHandler(IMigrateProviderMatchedLearnerDataService migrateProviderMatchedLearnerDataService, ILogger<MigrateProviderMatchedLearnerDataHandler> logger)
        {
            _migrateProviderMatchedLearnerDataService = migrateProviderMatchedLearnerDataService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(MigrateProviderMatchedLearnerData message, IMessageHandlerContext context)
        {
            try
            {
                await _migrateProviderMatchedLearnerDataService.MigrateProviderScopedData(message, context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error Handling Submission Succeeded Event, Please see internal exception for more info {exception}");
                throw;
            }
        }
    }
}

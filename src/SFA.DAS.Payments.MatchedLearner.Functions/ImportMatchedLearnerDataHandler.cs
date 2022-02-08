using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Payments.MatchedLearner.Application;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions
{
    public class ImportMatchedLearnerDataHandler : IHandleMessages<ImportMatchedLearnerData>
    {
        private readonly IMatchedLearnerDataImporter _matchedLearnerDataImporter;
        private readonly ILogger<ImportMatchedLearnerDataHandler> _logger;

        public ImportMatchedLearnerDataHandler(IMatchedLearnerDataImporter matchedLearnerDataImporter, ILogger<ImportMatchedLearnerDataHandler> logger)
        {
            _matchedLearnerDataImporter = matchedLearnerDataImporter ?? throw new ArgumentNullException(nameof(matchedLearnerDataImporter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task Handle(ImportMatchedLearnerData message, IMessageHandlerContext context)
        {
            try
            {
                await _matchedLearnerDataImporter.Import(message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error Handling ImportMatchedLearnerData, Inner Exception {exception}");
                throw;
            }
        }
    }
}
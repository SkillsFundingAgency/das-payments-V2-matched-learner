using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SFA.DAS.Payments.MatchedLearner.Application.Migration;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public class MatchedLearnerMigrationTrigger
    {
        private readonly IMatchedLearnerMigrationService _matchedLearnerMigrationService;

        public MatchedLearnerMigrationTrigger(IMatchedLearnerMigrationService matchedLearnerMigrationService)
        {
            _matchedLearnerMigrationService = matchedLearnerMigrationService;
        }

        [FunctionName("HttpTriggerMatchedLearnerMigration")]
        public async Task HttpTriggerMatchedLearnerMigration(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest httpRequest
        )
        {
            await _matchedLearnerMigrationService.TriggerMigrationForAllProviders();
        }
    }
}

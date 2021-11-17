using System;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public class MatchedLearnerDataImporter : IMatchedLearnerDataImporter
    {
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IMatchedLearnerDataImportService _matchedLearnerDataImportService;
        private readonly ILegacyMatchedLearnerDataImportService _legacyMatchedLearnerDataImportService;

        public MatchedLearnerDataImporter(IPaymentsRepository paymentsRepository, IMatchedLearnerDataImportService matchedLearnerDataImportService, ILegacyMatchedLearnerDataImportService legacyMatchedLearnerDataImportService)
        {
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
            _matchedLearnerDataImportService = matchedLearnerDataImportService ?? throw new ArgumentNullException(nameof(matchedLearnerDataImportService));
            _legacyMatchedLearnerDataImportService = legacyMatchedLearnerDataImportService ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerDataImportService));
        }

        public async Task Import(SubmissionJobSucceeded submissionSucceededEvent)
        {
            var dataLockEvents = await _paymentsRepository.GetDataLockEvents(submissionSucceededEvent);

            var legacyImportTask = _legacyMatchedLearnerDataImportService.Import(submissionSucceededEvent, dataLockEvents);
            
            var importTask = _matchedLearnerDataImportService.Import(submissionSucceededEvent, dataLockEvents);

            await Task.WhenAll(legacyImportTask, importTask);
        }
    }

    public interface IMatchedLearnerDataImporter
    {
        Task Import(SubmissionJobSucceeded submissionSucceededEvent);
    }
}

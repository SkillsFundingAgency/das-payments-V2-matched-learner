using System;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Data;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerDataImporter
    {
        Task Import(ImportMatchedLearnerData importMatchedLearnerData);
    }

    public class MatchedLearnerDataImporter : IMatchedLearnerDataImporter
    {
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IMatchedLearnerDataImportService _matchedLearnerDataImportService;
        private readonly ILegacyMatchedLearnerDataImportService _legacyMatchedLearnerDataImportService;

        public MatchedLearnerDataImporter(
            IMatchedLearnerRepository matchedLearnerRepository,
            IPaymentsRepository paymentsRepository, 
            IMatchedLearnerDataImportService matchedLearnerDataImportService, 
            ILegacyMatchedLearnerDataImportService legacyMatchedLearnerDataImportService)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
            _matchedLearnerDataImportService = matchedLearnerDataImportService ?? throw new ArgumentNullException(nameof(matchedLearnerDataImportService));
            _legacyMatchedLearnerDataImportService = legacyMatchedLearnerDataImportService ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerDataImportService));
        }

        public async Task Import(ImportMatchedLearnerData importMatchedLearnerData)
        {
            var dataLockEvents = await _paymentsRepository.GetDataLockEvents(importMatchedLearnerData);

            var dataLockEventSecondCopy = dataLockEvents.Clone();

            var legacyImportTask = _legacyMatchedLearnerDataImportService.Import(importMatchedLearnerData, dataLockEvents);

            var importTask = _matchedLearnerDataImportService.Import(importMatchedLearnerData, dataLockEventSecondCopy);

            await Task.WhenAll(legacyImportTask, importTask);
            
            await _matchedLearnerRepository.SaveSubmissionJob(new SubmissionJobModel
            {
                CollectionPeriod = importMatchedLearnerData.CollectionPeriod,
                DcJobId = importMatchedLearnerData.JobId,
                Ukprn = importMatchedLearnerData.Ukprn,
                AcademicYear = importMatchedLearnerData.AcademicYear,
                IlrSubmissionDateTime = importMatchedLearnerData.IlrSubmissionDateTime,
                EventTime = importMatchedLearnerData.EventTime
            });
        }
    }
}

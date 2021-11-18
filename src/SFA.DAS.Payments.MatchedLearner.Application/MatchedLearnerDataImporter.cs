using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerDataImporter
    {
        Task Import(SubmissionJobSucceeded submissionSucceededEvent);
    }

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

            var legacyImportTask = _legacyMatchedLearnerDataImportService.Import(submissionSucceededEvent, dataLockEvents.Clone());

            var importTask = _matchedLearnerDataImportService.Import(submissionSucceededEvent, dataLockEvents.Clone());

            await Task.WhenAll(legacyImportTask, importTask);
        }
    }

    //NOTE: Temp Code to allow both old and new Importers to work in parallel on same list
    public static class ListExtensions
    {
        public static List<T> Clone<T>(this List<T> oldList)
        {
            var serializeObject = JsonConvert.SerializeObject(oldList);
            return JsonConvert.DeserializeObject<List<T>>(serializeObject);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerDataImportService
    {
        Task Import(long ukprn, byte collectionPeriod, short academicYear);
    }

    public class MatchedLearnerDataImportService : IMatchedLearnerDataImportService
    {
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly string _paymentsRepository;

        public MatchedLearnerDataImportService(IMatchedLearnerRepository matchedLearnerRepository, string paymentsRepository)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
        }

        public async Task Import(long ukprn, byte collectionPeriod, short academicYear)
        {
            //if current collection is 1 then only try remove current collection data otherwise remove current + previous collection data
            var collectionPeriods = new List<byte> { collectionPeriod };
            if (collectionPeriod != 1)
            {
                collectionPeriods.Add((byte)(collectionPeriod - 1));
            }

            await _matchedLearnerRepository.RemovePreviousSubmissionData(ukprn, academicYear, collectionPeriods);

            //var datalockData = await _paymentsRepository.GetDataLockEvents(ukprn, academicYear, collectionPeriods);

            //await _matchedLearnerRepository.SaveDataLockEvents(datalockData);
        }
    }
}
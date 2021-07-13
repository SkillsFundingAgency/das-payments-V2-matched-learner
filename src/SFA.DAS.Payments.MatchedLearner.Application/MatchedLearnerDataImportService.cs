using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly IPaymentsRepository _paymentsRepository;

        public MatchedLearnerDataImportService(IMatchedLearnerRepository matchedLearnerRepository, IPaymentsRepository paymentsRepository)
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

            await _matchedLearnerRepository.RemovePreviousSubmissionsData(ukprn, academicYear, collectionPeriods);

            var datalockData = await _paymentsRepository.GetDataLockEvents(ukprn, academicYear, collectionPeriod);

            var apprenticeshipIds = datalockData
                .SelectMany(dle => dle.PayablePeriods)
                .Select(dlepp => dlepp.ApprenticeshipId.HasValue ? dlepp.ApprenticeshipId.Value : 0)
                .Union(datalockData.SelectMany(dle => dle.NonPayablePeriods).SelectMany(dlenpp => dlenpp.Failures)
                    .Select(dlenppf => dlenppf.ApprenticeshipId.HasValue ? dlenppf.ApprenticeshipId.Value : 0))
                .ToList();

            var apprenticeships = await _paymentsRepository.GetApprenticeships(apprenticeshipIds);

            await _matchedLearnerRepository.RemoveApprenticeships(apprenticeshipIds);

            await _matchedLearnerRepository.StoreApprenticeships(apprenticeships, CancellationToken.None);

            await _matchedLearnerRepository.StoreDataLocks(datalockData, CancellationToken.None);
        }
    }
}
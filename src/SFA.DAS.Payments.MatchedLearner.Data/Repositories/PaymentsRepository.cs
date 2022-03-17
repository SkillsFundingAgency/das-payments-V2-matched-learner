using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using ImportMatchedLearnerData = SFA.DAS.Payments.MatchedLearner.Data.Entities.ImportMatchedLearnerData;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IPaymentsRepository
    {
        Task<List<ApprenticeshipModel>> GetApprenticeships(List<long> ids);
        Task<List<DataLockEventModel>> GetDataLockEvents(ImportMatchedLearnerData importMatchedLearnerData);
    }

    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly PaymentsDataContext _paymentsDataContext;

        public PaymentsRepository(PaymentsDataContext paymentsDataContext)
        {
            _paymentsDataContext = paymentsDataContext ?? throw new ArgumentNullException(nameof(paymentsDataContext));
        }

        public async Task<List<ApprenticeshipModel>> GetApprenticeships(List<long> ids)
        {
            var apprenticeshipModels = new List<ApprenticeshipModel>();

            var apprenticeshipIdBatches = ids.Batch(2000);

            foreach (var batch in apprenticeshipIdBatches)
            {
                var apprenticeshipBatch = await _paymentsDataContext.Apprenticeship
                    .Where(a => batch.Contains(a.Id))
                    .ToListAsync();

                apprenticeshipModels.AddRange(apprenticeshipBatch);
            }

            return apprenticeshipModels;
        }

        public async Task<List<DataLockEventModel>> GetDataLockEvents(ImportMatchedLearnerData importMatchedLearnerData)
        {
            var transactionTypes = new List<byte> { 1, 2, 3 };

            var dataLockEvents = _paymentsDataContext.DataLockEvent
                    .Include(d => d.PriceEpisodes)
                    .Where(x =>
                        x.LearningAimReference == "ZPROG001" &&
                        x.AcademicYear == importMatchedLearnerData.AcademicYear &&
                        x.CollectionPeriod == importMatchedLearnerData.CollectionPeriod &&
                        x.JobId == importMatchedLearnerData.JobId &&
                        x.Ukprn == importMatchedLearnerData.Ukprn)
                    .OrderBy(x => x.LearningStartDate);

            var dataLockEventPayablePeriods = await _paymentsDataContext.DataLockEventPayablePeriod
                .Join(dataLockEvents, pp => pp.DataLockEventId, d => d.EventId, (pe, d) => pe)
                .Where(d => transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                .ToListAsync();

            var dataLockEventNonPayablePeriods = await _paymentsDataContext.DataLockEventNonPayablePeriod
                .Include(npp => npp.Failures)
                .Join(dataLockEvents, pp => pp.DataLockEventId, d => d.EventId, (pe, d) => pe)
                .Where(d => transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                .ToListAsync();

            var result = await dataLockEvents.ToListAsync();

            return result.Select(dle =>
            {
                dle.NonPayablePeriods = dataLockEventNonPayablePeriods
                    .Where(npp => npp.DataLockEventId == dle.EventId)
                    
                    .ToList();

                dle.PayablePeriods = dataLockEventPayablePeriods
                    .Where(pp => pp.DataLockEventId == dle.EventId)
                    .ToList();

                return dle;
            }).ToList();
        }
    }
}
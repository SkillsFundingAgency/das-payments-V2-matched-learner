using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IPaymentsRepository
    {
        Task<List<ApprenticeshipModel>> GetApprenticeships(List<long> ids);
        Task<List<DataLockEventModel>> GetDataLockEvents(SubmissionJobSucceeded submissionSucceededEvent);
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

        public async Task<List<DataLockEventModel>> GetDataLockEvents(SubmissionJobSucceeded submissionSucceededEvent)
        {
            var transactionTypes = new List<byte> { 1, 2, 3 };

            return await _paymentsDataContext.DataLockEvent
                .Include(d => d.NonPayablePeriods.Where(np => transactionTypes.Contains(np.TransactionType) && np.PriceEpisodeIdentifier != null && np.Amount != 0))
                .ThenInclude(npp => npp.Failures)
                .Include(d => d.PayablePeriods.Where(p => transactionTypes.Contains(p.TransactionType) && p.PriceEpisodeIdentifier != null && p.Amount != 0))
                .Include(d => d.PriceEpisodes)
                .Where(d =>
                    d.Ukprn == submissionSucceededEvent.Ukprn &&
                    d.AcademicYear == submissionSucceededEvent.AcademicYear &&
                    d.CollectionPeriod == submissionSucceededEvent.CollectionPeriod &&
                    d.JobId == submissionSucceededEvent.JobId &&
                    d.LearningAimReference == "ZPROG001")
                .ToListAsync();
        }
    }
}
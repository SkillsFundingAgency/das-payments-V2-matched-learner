using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IPaymentsRepository
    {
        Task<List<ApprenticeshipModel>> GetApprenticeships(List<long> ids);
        Task<List<DataLockEventModel>> GetDataLockEvents(long ukprn, short academicYear, byte collectionPeriod);
    }

    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly IPaymentsDataContext _paymentsDataContext;
        private readonly ILogger<PaymentsRepository> _logger;

        public PaymentsRepository(IPaymentsDataContext paymentsDataContext, ILogger<PaymentsRepository> logger)
        {
            _paymentsDataContext = paymentsDataContext ?? throw new ArgumentNullException(nameof(paymentsDataContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ApprenticeshipModel>> GetApprenticeships(List<long> ids)
        {
            var apprenticeshipModels = new List<ApprenticeshipModel>();

            var apprenticeshipIdBatches = Batch(ids, 2000);

            foreach (var batch in apprenticeshipIdBatches)
            {
                var apprenticeshipBatch = await _paymentsDataContext.Apprenticeship
                    .Where(a => batch.Contains(a.Id))
                    .ToListAsync();

                apprenticeshipModels.AddRange(apprenticeshipBatch);
            }

            return apprenticeshipModels;
        }



        public async Task<List<DataLockEventModel>> GetDataLockEvents(long ukprn, short academicYear, byte collectionPeriod)
        {
            return await _paymentsDataContext.DataLockEvent
                .Include(d => d.NonPayablePeriods)
                .ThenInclude(npp => npp.Failures)
                .Include(d => d.PayablePeriods)
                .Include(d => d.PriceEpisodes)
                .Where(d =>
                    d.Ukprn == ukprn &&
                    d.AcademicYear == academicYear &&
                    d.CollectionPeriod == collectionPeriod)
                .ToListAsync();
        }

        private IEnumerable<IEnumerable<T>> Batch<T>( IEnumerable<T> items, int maxItems)
        {
            return items
                .Select((item, inx) => new { item, inx })
                .GroupBy(x => x.inx / maxItems)
                .Select(g => g.Select(x => x.item));
        }
    }
}
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

            var dataLockEvents = await _paymentsDataContext.DataLockEvent
                .Where(x =>
                    x.LearningAimReference == "ZPROG001" &&
                    x.AcademicYear == submissionSucceededEvent.AcademicYear &&
                    x.CollectionPeriod == submissionSucceededEvent.CollectionPeriod &&
                    x.JobId == submissionSucceededEvent.JobId &&
                    x.Ukprn == submissionSucceededEvent.Ukprn)
                .OrderBy(x => x.LearningStartDate)
                .ToListAsync();

            if (dataLockEvents == null || !dataLockEvents.Any())
            {
                return new List<DataLockEventModel>();
            }

            var eventIds = dataLockEvents.Select(d => d.EventId).ToList();

            var dataLockEventPriceEpisodes = await _paymentsDataContext.DataLockEventPriceEpisode
                .Where(d => eventIds.Contains(d.DataLockEventId) && d.PriceEpisodeIdentifier != null)
                .OrderBy(p => p.StartDate)
                .ThenBy(p => p.PriceEpisodeIdentifier)
                .ToListAsync();

            var dataLockEventPayablePeriods = await _paymentsDataContext.DataLockEventPayablePeriod
                .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                .OrderBy(p => p.DeliveryPeriod)
                .ToListAsync();

            var dataLockEventNonPayablePeriods = await _paymentsDataContext.DataLockEventNonPayablePeriod
                .Where(d => eventIds.Contains(d.DataLockEventId) && transactionTypes.Contains(d.TransactionType) && d.PriceEpisodeIdentifier != null && d.Amount != 0)
                .OrderBy(p => p.DeliveryPeriod)
                .ToListAsync();

            var dataLockEventNonPayablePeriodIds = dataLockEventNonPayablePeriods.Select(d => d.DataLockEventNonPayablePeriodId).ToList();

            var dataLockEventNonPayablePeriodFailures = new List<DataLockEventNonPayablePeriodFailureModel>();
            if (dataLockEventNonPayablePeriodIds.Any())
            {
                dataLockEventNonPayablePeriodFailures = await _paymentsDataContext.DataLockEventNonPayablePeriodFailure
                .Where(d => dataLockEventNonPayablePeriodIds.Contains(d.DataLockEventNonPayablePeriodId))
                .ToListAsync();
            }

            return dataLockEvents.Select(dle =>
            {
                dle.NonPayablePeriods = dataLockEventNonPayablePeriods
                    .Where(npp => npp.DataLockEventId == dle.EventId)
                    .Select(npp =>
                    {
                        npp.Failures = dataLockEventNonPayablePeriodFailures
                            .Where(nppf =>
                                nppf.DataLockEventNonPayablePeriodId == npp.DataLockEventNonPayablePeriodId)
                            .ToList();

                        return npp;
                    })
                    .ToList();

                dle.PayablePeriods = dataLockEventPayablePeriods
                    .Where(pp => pp.DataLockEventId == dle.EventId)
                    .ToList();

                dle.PriceEpisodes = dataLockEventPriceEpisodes
                    .Where(pe => pe.DataLockEventId == dle.EventId)
                    .ToList();

                return dle;
            })
                .ToList();
        }
    }
}
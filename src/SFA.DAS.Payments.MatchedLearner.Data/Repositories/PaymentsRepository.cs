﻿using System;
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

            var basicDataLockEvents = _paymentsDataContext.DataLockEvent
                //.Include(d => d.NonPayablePeriods)
                //.ThenInclude(npp => npp.Failures)
                //.Include(d => d.PayablePeriods)
                .Include(d => d.PriceEpisodes)
                .Where(d =>
                    d.Ukprn == submissionSucceededEvent.Ukprn &&
                    d.AcademicYear == submissionSucceededEvent.AcademicYear &&
                    d.CollectionPeriod == submissionSucceededEvent.CollectionPeriod &&
                    d.JobId == submissionSucceededEvent.JobId &&
                    d.LearningAimReference == "ZPROG001").ToList();
            //.Select(d => new
            //{
            //    d.Id,
            //    d.EarningEventId,
            //    d.ContractType,
            //    d.AgreementId,
            //    d.IlrFileName,
            //    d.SfaContributionPercentage,
            //    d.EventType,
            //    d.IsPayable,
            //    d.DataLockSource,
            //    d.PriceEpisodes,
            //    d.EventId,
            //    NonPayablePeriods = d.NonPayablePeriods
            //        .Where(npp =>
            //            transactionTypes.Contains(npp.TransactionType) &&
            //            npp.PriceEpisodeIdentifier != null &&
            //            npp.Amount != 0),
            //PayablePeriods = d.PayablePeriods.Where(pp =>
            //    transactionTypes.Contains(pp.TransactionType) && pp.PriceEpisodeIdentifier != null &&
            //    pp.Amount != 0)
                //})
                //.ToListAsync();

            var basicDataLockEventsWithNonPayablePeriods = basicDataLockEvents.Join(
                _paymentsDataContext.DataLockEventNonPayablePeriod.Include(x => x.Failures).Where(npp =>
                    transactionTypes.Contains(npp.TransactionType) &&
                    npp.PriceEpisodeIdentifier != null &&
                    npp.Amount != 0),
                dle => dle.EventId,
                npp => npp.DataLockEventId,
                (dle, npp) => dle
            ).ToList();

            var dataLockEventsWithPayableAndNonPayablePeriods = basicDataLockEventsWithNonPayablePeriods.Join(
                _paymentsDataContext.DataLockEventPayablePeriod.Where(pp =>
                    transactionTypes.Contains(pp.TransactionType) && pp.PriceEpisodeIdentifier != null &&
                    pp.Amount != 0),
                dle => dle.EventId,
                pp => pp.DataLockEventId,
                (dle, pp) => dle
            ).ToList();

            return dataLockEventsWithPayableAndNonPayablePeriods;

            //return result.Select(d => new DataLockEventModel
            //{
            //    Id = d.Id,
            //    EarningEventId = d.EarningEventId,
            //    ContractType = d.ContractType,
            //    AgreementId = d.AgreementId,
            //    IlrFileName = d.IlrFileName,
            //    SfaContributionPercentage = d.SfaContributionPercentage,
            //    EventType = d.EventType,
            //    IsPayable = d.IsPayable,
            //    DataLockSource = d.DataLockSource,
            //    PriceEpisodes = d.PriceEpisodes,
            //    NonPayablePeriods = d.NonPayablePeriods.ToList(),
            //    PayablePeriods = d.PayablePeriods.ToList()
            //}).ToList();
        }
    }
}
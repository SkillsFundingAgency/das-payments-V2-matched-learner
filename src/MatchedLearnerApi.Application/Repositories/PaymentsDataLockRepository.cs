using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchedLearnerApi.Application.Data;
using MatchedLearnerApi.Application.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchedLearnerApi.Application.Repositories
{
    public interface IPaymentsDataLockRepository
    {
        Task<List<DatalockEvent>> GetDatalockEvents(long ukprn, long uln);
    }

    public class PaymentsDataLockRepository : IPaymentsDataLockRepository
    {
        private readonly IPaymentsContext _context;

        public PaymentsDataLockRepository(IPaymentsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<DatalockEvent>> GetDatalockEvents(long ukprn, long uln)
        {
            var latestSuccessfulJob = await _context.LatestSuccessfulJobs
                .Where(y => y.Ukprn == ukprn)
                .OrderByDescending(y => y.AcademicYear)
                .ThenByDescending(y => y.CollectionPeriod)
                .FirstOrDefaultAsync();

            if (latestSuccessfulJob == null)
                return new List<DatalockEvent>();

            var transactionTypes = new List<byte> { 1, 2, 3 };

            var datalockEvents = await _context.DatalockEvents
                .Include(d => d.PriceEpisodes)
                .Include(d => d.PayablePeriods).ThenInclude(pp => pp.Apprenticeship)
                .Include(d => d.NonPayablePeriods).ThenInclude(npp => npp.Failures).ThenInclude(f => f.Apprenticeship)
                .Where(x => x.LearningAimReference == "ZPROG001")
                .Where(x => x.Ukprn == ukprn && x.LearnerUln == uln)
                .Where(x => x.LearnerUln == uln)
                .Where(x => x.JobId == latestSuccessfulJob.DcJobId)
                .Where(x => x.AcademicYear == latestSuccessfulJob.AcademicYear)
                .Where(x => x.CollectionPeriod == latestSuccessfulJob.CollectionPeriod)
                .OrderBy(x => x.LearningStartDate)
                .Select(d => new DatalockEvent
                {
                    NonPayablePeriods = d.NonPayablePeriods.Where(npp => transactionTypes.Contains(npp.TransactionType) && npp.PriceEpisodeIdentifier != null).ToList(),
                    PayablePeriods = d.PayablePeriods.Where(pp => transactionTypes.Contains(pp.TransactionType) && pp.PriceEpisodeIdentifier != null).ToList(),
                    PriceEpisodes = d.PriceEpisodes,

                    Ukprn = d.Ukprn,
                    LearnerUln = d.LearnerUln,

                    AcademicYear = d.AcademicYear,
                    CollectionPeriod = d.CollectionPeriod,

                    LearningAimReference = d.LearningAimReference,

                    LearningAimPathwayCode = d.LearningAimPathwayCode,
                    LearningAimStandardCode = d.LearningAimStandardCode,
                    LearningAimFrameworkCode = d.LearningAimFrameworkCode,
                    LearningAimFundingLineType = d.LearningAimFundingLineType,
                    LearningAimProgrammeType = d.LearningAimProgrammeType,

                    IlrSubmissionDateTime = d.IlrSubmissionDateTime,
                    LearningStartDate = d.LearningStartDate,
                    EventTime = d.EventTime,
                })
                .ToListAsync();

            return GroupDatalockEvents(datalockEvents);
        }

        private static List<DatalockEvent> GroupDatalockEvents(IEnumerable<DatalockEvent> datalockEvents)
        {
            var result = datalockEvents.GroupBy(x => new
            {
                x.LearningAimReference,
                x.LearningAimStandardCode,
                x.LearningAimProgrammeType,
                x.LearningAimFrameworkCode,
                x.LearningAimPathwayCode,
                x.AcademicYear,
                x.LearningAimFundingLineType,
                x.LearningStartDate,
                x.LearnerUln,
                x.Ukprn,
                x.CollectionPeriod,
                x.IlrSubmissionDateTime,
            }).Select(d => new DatalockEvent
            {
                NonPayablePeriods = d.SelectMany(p => p.NonPayablePeriods).OrderBy(p => p.DeliveryPeriod).ToList(),
                PayablePeriods = d.SelectMany(p => p.PayablePeriods).OrderBy(p => p.DeliveryPeriod).ToList(),
                PriceEpisodes = GroupPriceEpisodes(d.SelectMany(p => p.PriceEpisodes)),

                Ukprn = d.Key.Ukprn,
                LearnerUln = d.Key.LearnerUln,

                AcademicYear = d.Key.AcademicYear,
                CollectionPeriod = d.Key.CollectionPeriod,

                LearningAimReference = d.Key.LearningAimReference,

                LearningAimPathwayCode = d.Key.LearningAimPathwayCode,
                LearningAimStandardCode = d.Key.LearningAimStandardCode,
                LearningAimFrameworkCode = d.Key.LearningAimFrameworkCode,
                LearningAimFundingLineType = d.Key.LearningAimFundingLineType,
                LearningAimProgrammeType = d.Key.LearningAimProgrammeType,

                IlrSubmissionDateTime = d.Key.IlrSubmissionDateTime,
                LearningStartDate = d.Key.LearningStartDate,
            }).ToList();


            return result;
        }

        private static List<DatalockEventPriceEpisode> GroupPriceEpisodes(IEnumerable<DatalockEventPriceEpisode> priceEpisodes)
        {
            var result = priceEpisodes.GroupBy(pe => new
            {
                pe.PriceEpisodeIdentifier,
                pe.ActualEndDate,
                pe.Completed,
                pe.CompletionAmount,
                pe.InstalmentAmount,
                pe.NumberOfInstalments,
                pe.StartDate,
                pe.TotalNegotiatedPrice1,
                pe.TotalNegotiatedPrice2,
                pe.TotalNegotiatedPrice3,
                pe.TotalNegotiatedPrice4,
            }).Select(pe => new DatalockEventPriceEpisode
            {
                PriceEpisodeIdentifier = pe.Key.PriceEpisodeIdentifier,
                ActualEndDate = pe.Key.ActualEndDate,
                Completed = pe.Key.Completed,
                CompletionAmount = pe.Key.CompletionAmount,
                InstalmentAmount = pe.Key.InstalmentAmount,
                NumberOfInstalments = pe.Key.NumberOfInstalments,
                StartDate = pe.Key.StartDate,
                TotalNegotiatedPrice1 = pe.Key.TotalNegotiatedPrice1,
                TotalNegotiatedPrice2 = pe.Key.TotalNegotiatedPrice2,
                TotalNegotiatedPrice3 = pe.Key.TotalNegotiatedPrice3,
                TotalNegotiatedPrice4 = pe.Key.TotalNegotiatedPrice4,
            })
            .OrderBy(pe => pe.StartDate)
            .ToList();

            return result;
        }
    }
}

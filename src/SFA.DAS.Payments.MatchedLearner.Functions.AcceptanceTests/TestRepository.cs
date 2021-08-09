using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
	public class TestRepository : IDisposable
	{
		private readonly PaymentsDataContext _paymentsDataContext;
		private readonly MatchedLearnerDataContext _matchedLearnerDataContext;

		public TestRepository()
		{
			var applicationSettings = TestConfiguration.ApplicationSettings;

			var matchedLearnerOptions = new DbContextOptionsBuilder()
				.UseSqlServer(applicationSettings.MatchedLearnerConnectionString)
				.Options;

			_matchedLearnerDataContext = new MatchedLearnerDataContext(matchedLearnerOptions);


			var paymentsOptions = new DbContextOptionsBuilder()
				.UseSqlServer(applicationSettings.PaymentsConnectionString)
				.Options;

			_paymentsDataContext = new PaymentsDataContext(paymentsOptions);

		}

        public async Task<Guid> AddDataLockEvent(long ukprn, long uln, byte collectionPeriod, short academicYear, bool useMatchedLearnerContext)
		{
			const string sql = @"
            declare @testDateTime as DateTimeOffset = SysDateTimeOffset()

            INSERT INTO Payments2.Apprenticeship (Id, AccountId, AgreedOnDate, Uln, Ukprn, EstimatedStartDate, EstimatedEndDate, Priority, StandardCode, ProgrammeType, FrameworkCode, PathwayCode, TransferSendingEmployerAccountId, Status, IsLevyPayer, ApprenticeshipEmployerType)
            VALUES (123456, 1000, @testDateTime, @uln, @ukprn, @testDateTime, @testDateTime, 1, 100, 200, 300, 400, 500, 0, 0, 3)

            INSERT INTO Payments2.DataLockEvent (EventId, EarningEventId, Ukprn, ContractType, CollectionPeriod, AcademicYear, LearnerReferenceNumber, LearnerUln, LearningAimReference, LearningAimProgrammeType, LearningAimStandardCode, LearningAimFrameworkCode, LearningAimPathwayCode, LearningAimFundingLineType, IlrSubmissionDateTime, IsPayable, DataLockSourceId, JobId, EventTime, LearningStartDate)
            VALUES (@dataLockEventId, NewID(), @ukprn, 1, @collectionPeriod, @academicYear, 'ref#', @uln, 'ZPROG001', 100, 200, 300, 400, 'funding', '2020-10-10', 0, 0, 123, @testDateTime, '2020-10-09 0:00 +00:00')

            INSERT INTO Payments2.DataLockEventPriceEpisode (DataLockEventId, PriceEpisodeIdentifier, SfaContributionPercentage, TotalNegotiatedPrice1, TotalNegotiatedPrice2, TotalNegotiatedPrice3, TotalNegotiatedPrice4, StartDate, EffectiveTotalNegotiatedPriceStartDate, PlannedEndDate, ActualEndDate, NumberOfInstalments, InstalmentAmount, CompletionAmount, Completed)
            VALUES (@dataLockEventId, '25-104-01/08/2020', 1, 1000, 2000, 0, 0, '2020-10-07', '2021-01-01', '2020-10-11', '2020-10-12', 12, 50, 550, 0)

            INSERT INTO Payments2.DataLockEventPayablePeriod (DataLockEventId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage, LearningStartDate, ApprenticeshipId)
            VALUES  (@dataLockEventId, '25-104-01/08/2020', 1, 1, 100, 1, @testDateTime, 123456),
                    (@dataLockEventId, '25-104-01/08/2020', 1, 2, 200, 1, @testDateTime, 123456),
                    (@dataLockEventId, '25-104-01/08/2020', 1, 3, 300, 1, @testDateTime, 123456)

            INSERT INTO Payments2.DataLockEventNonPayablePeriod (DataLockEventId, DataLockEventNonPayablePeriodId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage)
            VALUES  (@dataLockEventId, @dataLockEventFailureId1, '25-104-01/08/2020', 1, 3, 400, 1),
                    (@dataLockEventId, @dataLockEventFailureId2, '25-104-01/08/2020', 1, 4, 500, 1),
                    (@dataLockEventId, @dataLockEventFailureId3, '25-104-01/08/2020', 1, 5, 600, 1),
                    (@dataLockEventId, @dataLockEventFailureId4, '25-104-01/08/2020', 1, 6, 600, 1)

            INSERT INTO Payments2.DataLockEventNonPayablePeriodFailures (DataLockEventNonPayablePeriodId, DataLockFailureId, ApprenticeshipId)
            VALUES  (@dataLockEventFailureId1, 1, 123456), 
                    (@dataLockEventFailureId1, 2, 123456), 
                    (@dataLockEventFailureId1, 3, 123456), 
                    (@dataLockEventFailureId2, 7, 123456), 
                    (@dataLockEventFailureId3, 9, 123456),
                    (@dataLockEventFailureId4, 1, 12345600)
            ";

			var dataLockEventId = Guid.NewGuid();
			var dataLockEventFailureId1 = Guid.NewGuid();
			var dataLockEventFailureId2 = Guid.NewGuid();
			var dataLockEventFailureId3 = Guid.NewGuid();
            var dataLockEventFailureId4 = Guid.NewGuid();


			var database = useMatchedLearnerContext ? _matchedLearnerDataContext.Database : _paymentsDataContext.Database;

            await database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("ukprn", ukprn),
                    new SqlParameter("uln", uln),
                    new SqlParameter("dataLockEventId", dataLockEventId),
                    new SqlParameter("dataLockEventFailureId1", dataLockEventFailureId1),
                    new SqlParameter("dataLockEventFailureId2", dataLockEventFailureId2),
                    new SqlParameter("dataLockEventFailureId3", dataLockEventFailureId3),
                    new SqlParameter("dataLockEventFailureId4", dataLockEventFailureId4),
					new SqlParameter("collectionPeriod", collectionPeriod),
                    new SqlParameter("academicYear", academicYear)
                );

            return dataLockEventId;
        }

		public async Task ClearDataLockEvent(long ukprn, long uln)
		{
			await ClearPaymentDataLockEvent(ukprn, uln);
			await ClearMatchedLearnerDataLockEvent(ukprn, uln);
		}

		const string ClearDataLockEventSql = @"
            DELETE Payments2.Apprenticeship WHERE Uln = @uln AND Ukprn = @ukprn;
            DELETE Payments2.Apprenticeship WHERE Id = 123456;

            DELETE Payments2.DataLockEventPayablePeriod
            WHERE DataLockEventId IN (
                SELECT EventId 
                FROM Payments2.DataLockEvent
                WHERE LearnerUln = @uln
                AND Ukprn = @ukprn
            )

            DELETE Payments2.DataLockEventNonPayablePeriodFailures
            WHERE DataLockEventNonPayablePeriodId IN (
	            SELECT DataLockEventNonPayablePeriodId
	            FROM Payments2.DataLockEventNonPayablePeriod
	            WHERE DataLockEventId IN (
		            SELECT EventId 
		            FROM Payments2.DataLockEvent
		            WHERE LearnerUln = @uln
		            AND Ukprn = @ukprn
	            )
            )

            DELETE Payments2.DataLockEventNonPayablePeriod
            WHERE DataLockEventId IN (
	            SELECT EventId 
	            FROM Payments2.DataLockEvent
	            WHERE LearnerUln = @uln
	            AND Ukprn = @ukprn
            )

            DELETE Payments2.DataLockEventPriceEpisode
            WHERE DataLockEventId IN (
	            SELECT EventId 
	            FROM Payments2.DataLockEvent
	            WHERE LearnerUln = @uln
	            AND Ukprn = @ukprn
            )

            DELETE Payments2.DataLockEvent
            WHERE LearnerUln = @uln
            AND Ukprn = @ukprn 
            ";

		private async Task ClearPaymentDataLockEvent(long ukprn, long uln)
		{
			await _paymentsDataContext.Database.ExecuteSqlRawAsync(ClearDataLockEventSql, new SqlParameter("ukprn", ukprn), new SqlParameter("uln", uln));
		}

		private async Task ClearMatchedLearnerDataLockEvent(long ukprn, long uln)
		{
			await _matchedLearnerDataContext.Database.ExecuteSqlRawAsync(ClearDataLockEventSql, new SqlParameter("ukprn", ukprn), new SqlParameter("uln", uln));
		}

		public async Task<List<DataLockEventModel>> GetMatchedLearnerDataLockEvents(long ukprn)
		{
			return await _matchedLearnerDataContext.DataLockEvent
				.Include(d => d.NonPayablePeriods)
				.ThenInclude(npp => npp.Failures)
				.Include(d => d.PayablePeriods)
				.Include(d => d.PriceEpisodes)
				.Where(d =>
					d.Ukprn == ukprn)
				.ToListAsync();
		}

		public void Dispose()
		{
			_paymentsDataContext?.Dispose();
			_matchedLearnerDataContext?.Dispose();
		}
	}
}

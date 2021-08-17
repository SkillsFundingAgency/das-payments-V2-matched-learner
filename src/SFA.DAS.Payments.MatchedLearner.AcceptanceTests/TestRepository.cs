using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests
{
	public class TestRepository
	{
		private readonly MatchedLearnerDataContext _matchedLearnerDataContext;

		public TestRepository()
		{
			var applicationSettings = TestConfiguration.TestApplicationSettings;

			var matchedLearnerOptions = new DbContextOptionsBuilder()
				.UseSqlServer(applicationSettings.MatchedLearnerAcceptanceTestConnectionString)
				.Options;

			_matchedLearnerDataContext = new MatchedLearnerDataContext(matchedLearnerOptions);
		}

		public async Task AddDataLockEvent(long ukprn, long uln)
		{
			const string sql = @"
            declare @testDateTime as DateTimeOffset = SysDateTimeOffset()

            INSERT INTO Payments2.Apprenticeship (Id, AccountId, AgreedOnDate, Uln, Ukprn, EstimatedStartDate, EstimatedEndDate, Priority, StandardCode, ProgrammeType, FrameworkCode, PathwayCode, TransferSendingEmployerAccountId, Status, IsLevyPayer, ApprenticeshipEmployerType)
            VALUES (@apprenticeshipId, 1000, @testDateTime, @uln, @ukprn, @testDateTime, @testDateTime, 1, 100, 200, 300, 400, 500, 0, 0, 3)

            INSERT INTO Payments2.DataLockEvent (EventId, EarningEventId, Ukprn, ContractType, CollectionPeriod, AcademicYear, LearnerReferenceNumber, LearnerUln, LearningAimReference, LearningAimProgrammeType, LearningAimStandardCode, LearningAimFrameworkCode, LearningAimPathwayCode, LearningAimFundingLineType, IlrSubmissionDateTime, IsPayable, DataLockSourceId, JobId, EventTime, LearningStartDate)
            VALUES (@dataLockEventId1, NewID(), @ukprn, 1, 14, 1920, 'ref#', @uln, 'ZPROG001', 100, 200, 300, 400, 'funding', '2020-10-10', 0, 0, 456, @testDateTime, '2020-10-09 0:00 +00:00')

            INSERT INTO Payments2.DataLockEventPriceEpisode (DataLockEventId, PriceEpisodeIdentifier, SfaContributionPercentage, TotalNegotiatedPrice1, TotalNegotiatedPrice2, TotalNegotiatedPrice3, TotalNegotiatedPrice4, StartDate, EffectiveTotalNegotiatedPriceStartDate, PlannedEndDate, ActualEndDate, NumberOfInstalments, InstalmentAmount, CompletionAmount, Completed)
            VALUES (@dataLockEventId1, '25-104-01/08/2019', 1, 1000, 2000, 0, 0, '2020-10-07', '2021-01-01', '2020-10-11', '2020-10-12', 12, 50, 550, 0)
            
            INSERT INTO Payments2.DataLockEventPayablePeriod (DataLockEventId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage, LearningStartDate, ApprenticeshipId)
            VALUES  (@dataLockEventId1, '25-104-01/08/2019', 1, 1, 100, 1, @testDateTime, @apprenticeshipId),
                    (@dataLockEventId1, '25-104-01/08/2019', 1, 2, 200, 1, @testDateTime, @apprenticeshipId),
                    (@dataLockEventId1, '25-104-01/08/2019', 1, 3, 300, 1, @testDateTime, @apprenticeshipId)





            INSERT INTO Payments2.DataLockEvent (EventId, EarningEventId, Ukprn, ContractType, CollectionPeriod, AcademicYear, LearnerReferenceNumber, LearnerUln, LearningAimReference, LearningAimProgrammeType, LearningAimStandardCode, LearningAimFrameworkCode, LearningAimPathwayCode, LearningAimFundingLineType, IlrSubmissionDateTime, IsPayable, DataLockSourceId, JobId, EventTime, LearningStartDate)
            VALUES (@dataLockEventId2, NewID(), @ukprn, 1, 1, 2021, 'ref#', @uln, 'ZPROG001', 100, 200, 300, 400, 'funding', '2020-10-10', 0, 0, 123, @testDateTime, '2020-10-09 0:00 +00:00')

            INSERT INTO Payments2.DataLockEventPriceEpisode (DataLockEventId, PriceEpisodeIdentifier, SfaContributionPercentage, TotalNegotiatedPrice1, TotalNegotiatedPrice2, TotalNegotiatedPrice3, TotalNegotiatedPrice4, StartDate, EffectiveTotalNegotiatedPriceStartDate, PlannedEndDate, ActualEndDate, NumberOfInstalments, InstalmentAmount, CompletionAmount, Completed)
            VALUES (@dataLockEventId2, '25-104-01/08/2020', 1, 1000, 2000, 0, 0, '2020-10-07', '2021-01-01', '2020-10-11', '2020-10-12', 12, 50, 550, 0)

            INSERT INTO Payments2.DataLockEventPayablePeriod (DataLockEventId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage, LearningStartDate, ApprenticeshipId)
            VALUES  (@dataLockEventId2, '25-104-01/08/2020', 1, 1, 100, 1, @testDateTime, @apprenticeshipId),
                    (@dataLockEventId2, '25-104-01/08/2020', 1, 2, 200, 1, @testDateTime, @apprenticeshipId),
                    (@dataLockEventId2, '25-104-01/08/2020', 1, 3, 300, 1, @testDateTime, @apprenticeshipId)

            INSERT INTO Payments2.DataLockEventNonPayablePeriod (DataLockEventId, DataLockEventNonPayablePeriodId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage)
            VALUES  (@dataLockEventId2, @dataLockEventFailureId1, '25-104-01/08/2020', 1, 3, 400, 1),
                    (@dataLockEventId2, @dataLockEventFailureId2, '25-104-01/08/2020', 1, 4, 500, 1),
                    (@dataLockEventId2, @dataLockEventFailureId3, '25-104-01/08/2020', 1, 5, 600, 1),
                    (@dataLockEventId2, @dataLockEventFailureId4, '25-104-01/08/2020', 1, 6, 600, 1)

            INSERT INTO Payments2.DataLockEventNonPayablePeriodFailures (DataLockEventNonPayablePeriodId, DataLockFailureId, ApprenticeshipId)
            VALUES  (@dataLockEventFailureId1, 1, @apprenticeshipId), 
                    (@dataLockEventFailureId1, 2, @apprenticeshipId), 
                    (@dataLockEventFailureId1, 3, @apprenticeshipId), 
                    (@dataLockEventFailureId2, 7, @apprenticeshipId), 
                    (@dataLockEventFailureId3, 9, @apprenticeshipId),
                    (@dataLockEventFailureId4, 1, 12345600)
            ";

			var dataLockEventId1 = Guid.NewGuid();
			var dataLockEventId2 = Guid.NewGuid();
			var dataLockEventFailureId1 = Guid.NewGuid();
			var dataLockEventFailureId2 = Guid.NewGuid();
			var dataLockEventFailureId3 = Guid.NewGuid();
			var dataLockEventFailureId4 = Guid.NewGuid();

			var apprenticeshipId = ukprn + uln;

			await _matchedLearnerDataContext.Database.ExecuteSqlRawAsync(sql,
				new SqlParameter("apprenticeshipId", apprenticeshipId),
				new SqlParameter("ukprn", ukprn),
				new SqlParameter("uln", uln),
				new SqlParameter("dataLockEventId1", dataLockEventId1),
				new SqlParameter("dataLockEventId2", dataLockEventId2),
				new SqlParameter("dataLockEventFailureId1", dataLockEventFailureId1),
				new SqlParameter("dataLockEventFailureId2", dataLockEventFailureId2),
				new SqlParameter("dataLockEventFailureId3", dataLockEventFailureId3),
				new SqlParameter("dataLockEventFailureId4", dataLockEventFailureId4));
		}

		public async Task ClearLearner(long ukprn, long uln)
		{
			const string sql = @"
            DELETE Payments2.Apprenticeship WHERE Uln = @uln AND Ukprn = @ukprn;
            DELETE Payments2.Apprenticeship WHERE id = @apprenticeshipId;

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

			var apprenticeshipId = ukprn + uln;

			await _matchedLearnerDataContext.Database.ExecuteSqlRawAsync(sql,
				new SqlParameter("apprenticeshipId", apprenticeshipId),
				new SqlParameter("ukprn", ukprn),
				new SqlParameter("uln", uln));
		}
	}
}

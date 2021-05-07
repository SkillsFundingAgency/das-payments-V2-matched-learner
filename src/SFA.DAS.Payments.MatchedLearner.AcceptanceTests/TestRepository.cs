using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests
{
    public class TestRepository
    {
        private readonly string _connectionString;

        public TestRepository()
        {
            _connectionString = TestConfiguration.MatchedLearnerApiConfiguration.DasPaymentsDatabaseConnectionString;
        }

        public async Task AddDataLockEvent(long ukprn, long uln)
        {
            const string sql = @"

INSERT INTO Payments2.Job (IlrSubmissionTime,   JobType, Status, DCJobId, Ukprn, AcademicYear, CollectionPeriod, DCJobSucceeded)
                   VALUES (SysDateTimeOffset(), 1,       2,       123,      @ukprn, 2021,          1,                 1)

INSERT INTO Payments2.Apprenticeship (Id, AccountId, AgreedOnDate, Uln, Ukprn, EstimatedStartDate, EstimatedEndDate,
    Priority, StandardCode, ProgrammeType, FrameworkCode, PathwayCode, TransferSendingEmployerAccountId, Status,
    IsLevyPayer, ApprenticeshipEmployerType)
VALUES (-123456, 1000, SysDateTimeOffset(), @uln, @ukprn, SysDateTimeOffset(), SysDateTimeOffset(), 1, 100, 200, 300, 400, 500, 0, 0, 3)

INSERT INTO Payments2.DataLockEvent (EventId, EarningEventId, Ukprn, ContractType, CollectionPeriod, AcademicYear,
    LearnerReferenceNumber, LearnerUln, LearningAimReference, LearningAimProgrammeType, LearningAimStandardCode,
    LearningAimFrameworkCode, LearningAimPathwayCode, LearningAimFundingLineType, IlrSubmissionDateTime, IsPayable,
    DataLockSourceId, JobId, EventTime, LearningStartDate)
VALUES (@dataLockEventId, NewID(), @ukprn, 1, 1, 2021, 'ref#', @uln, 'ZPROG001', 100, 200, 300, 400, 'funding', 
    '2020-10-10', 0, 0, 123, SysDateTimeOffset(), '2020-10-09 0:00 +00:00')


INSERT INTO Payments2.DataLockEventPayablePeriod (DataLockEventId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod,
    Amount, SfaContributionPercentage, LearningStartDate, ApprenticeshipId)
VALUES  (@dataLockEventId, 'TEST', 1, 1, 100, 1, SysDateTimeOffset(), -123456),
        (@dataLockEventId, 'TEST', 1, 2, 200, 1, SysDateTimeOffset(), -123456),
        (@dataLockEventId, 'TEST', 1, 3, 300, 1, SysDateTimeOffset(), -123456)


INSERT INTO Payments2.DataLockEventNonPayablePeriod (DataLockEventId, DataLockEventNonPayablePeriodId, PriceEpisodeIdentifier, 
    TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage)
VALUES  (@dataLockEventId, @dataLockEventFailureId1, 'TEST', 1, 3, 400, 1),
        (@dataLockEventId, @dataLockEventFailureId2, 'TEST', 1, 4, 500, 1),
        (@dataLockEventId, @dataLockEventFailureId3, 'TEST', 1, 5, 600, 1),
        (@dataLockEventId, @dataLockEventFailureId4, 'TEST', 1, 6, 600, 1)

INSERT INTO Payments2.DataLockEventNonPayablePeriodFailures (DataLockEventNonPayablePeriodId, DataLockFailureId, ApprenticeshipId)
VALUES  (@dataLockEventFailureId1, 1, -123456), 
        (@dataLockEventFailureId1, 2, -123456), 
        (@dataLockEventFailureId1, 3, -123456), 
        (@dataLockEventFailureId2, 7, -123456), 
        (@dataLockEventFailureId3, 9, -123456),
        (@dataLockEventFailureId4, 1, -12345600)

INSERT INTO Payments2.DataLockEventPriceEpisode (DataLockEventId, PriceEpisodeIdentifier, SfaContributionPercentage,
    TotalNegotiatedPrice1, TotalNegotiatedPrice2, TotalNegotiatedPrice3, TotalNegotiatedPrice4, StartDate,
    EffectiveTotalNegotiatedPriceStartDate, PlannedEndDate, ActualEndDate, NumberOfInstalments, InstalmentAmount, CompletionAmount,
    Completed)
VALUES (@dataLockEventId, 'TEST', 1, 1000, 2000, 0, 0, '2020-10-07', SysDateTimeOffset(), '2020-10-11', '2020-10-12', 12, 50, 550, 0)


";

            var dataLockEventId = Guid.NewGuid();
            var dataLockEventFailureId1 = Guid.NewGuid();
            var dataLockEventFailureId2 = Guid.NewGuid();
            var dataLockEventFailureId3 = Guid.NewGuid();
            var dataLockEventFailureId4 = Guid.NewGuid();

            await using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(sql, new
            {
                ukprn, uln, dataLockEventId,
                dataLockEventFailureId1, dataLockEventFailureId2, dataLockEventFailureId3, dataLockEventFailureId4
            });
        }

        public async Task ClearLearner(long ukprn, long uln)
        {
            const string sql = @"
DELETE FROM Payments2.Job where Ukprn = @ukprn
DELETE Payments2.Apprenticeship WHERE Uln = @uln AND Ukprn = @ukprn;
DELETE Payments2.Apprenticeship WHERE Id = -123456;

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

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new {ukprn, uln});
        }
    }
}

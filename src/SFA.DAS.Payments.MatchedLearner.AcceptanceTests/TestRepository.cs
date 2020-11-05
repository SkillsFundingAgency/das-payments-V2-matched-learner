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

        public async Task AddDatalockEvent(long ukprn, long uln)
        {
            const string sql = @"

INSERT INTO Payments2.Job (IlrSubmissionTime,   JobType, Status, DCJobId, Ukprn, AcademicYear, CollectionPeriod, DCJobSucceeded)
                   VALUES (SysDateTimeOffset(), 1,       2,       123,      @ukprn, 2021,          1,                 1)

INSERT INTO Payments2.Apprenticeship (Id, AccountId, AgreedOnDate, Uln, Ukprn, EstimatedStartDate, EstimatedEndDate,
    Priority, StandardCode, ProgrammeType, FrameworkCode, PathwayCode, TransferSendingEmployerAccountId, Status,
    IsLevyPayer, ApprenticeshipEmployerType)
VALUES (-123456, 1000, SysDateTimeOffset(), @uln, @ukprn, SysDateTimeOffset(), SysDateTimeOffset(), 1, 100, 200, 300, 400, 500, 0, 0, 3)

INSERT INTO Payments2.DatalockEvent (EventId, EarningEventId, Ukprn, ContractType, CollectionPeriod, AcademicYear,
    LearnerReferenceNumber, LearnerUln, LearningAimReference, LearningAimProgrammeType, LearningAimStandardCode,
    LearningAimFrameworkCode, LearningAimPathwayCode, LearningAimFundingLineType, IlrSubmissionDateTime, IsPayable,
    DatalockSourceId, JobId, EventTime, LearningStartDate)
VALUES (@datalockEventId, NewID(), @ukprn, 1, 1, 2021, 'ref#', @uln, 'ZPROG001', 100, 200, 300, 400, 'funding', 
    '2020-10-10', 0, 0, 123, SysDateTimeOffset(), '2020-10-09 0:00 +00:00')


INSERT INTO Payments2.DatalockEventPayablePeriod (DatalockEventId, PriceEpisodeIdentifier, TransactionType, DeliveryPeriod,
    Amount, SfaContributionPercentage, LearningStartDate, ApprenticeshipId)
VALUES  (@datalockEventId, 'TEST', 1, 1, 100, 1, SysDateTimeOffset(), -123456),
        (@datalockEventId, 'TEST', 1, 2, 200, 1, SysDateTimeOffset(), -123456),
        (@datalockEventId, 'TEST', 1, 3, 300, 1, SysDateTimeOffset(), -123456)


INSERT INTO Payments2.DatalockEventNonPayablePeriod (DatalockEventId, DataLockEventNonPayablePeriodId, PriceEpisodeIdentifier, 
    TransactionType, DeliveryPeriod, Amount, SfaContributionPercentage)
VALUES  (@datalockEventId, @datalockEventFailureId1, 'TEST', 1, 3, 400, 1),
        (@datalockEventId, @datalockEventFailureId2, 'TEST', 1, 4, 500, 1),
        (@datalockEventId, @datalockEventFailureId3, 'TEST', 1, 5, 600, 1),
        (@datalockEventId, @datalockEventFailureId4, 'TEST', 1, 6, 600, 1)

INSERT INTO Payments2.DataLockEventNonPayablePeriodFailures (DatalockEventNonPayablePeriodId, DatalockFailureId, ApprenticeshipId)
VALUES  (@datalockEventFailureId1, 1, -123456), 
        (@datalockEventFailureId1, 2, -123456), 
        (@datalockEventFailureId1, 3, -123456), 
        (@datalockEventFailureId2, 7, -123456), 
        (@datalockEventFailureId3, 9, -123456),
        (@datalockEventFailureId4, 1, -12345600)

INSERT INTO Payments2.DatalockEventPriceEpisode (DatalockEventId, PriceEpisodeIdentifier, SfaContributionPercentage,
    TotalNegotiatedPrice1, TotalNegotiatedPrice2, TotalNegotiatedPrice3, TotalNegotiatedPrice4, StartDate,
    EffectiveTotalNegotiatedPriceStartDate, PlannedEndDate, ActualEndDate, NumberOfInstalments, InstalmentAmount, CompletionAmount,
    Completed)
VALUES (@datalockEventId, 'TEST', 1, 1000, 2000, 0, 0, '2020-10-07', SysDateTimeOffset(), '2020-10-11', '2020-10-12', 12, 50, 550, 0)


";

            var datalockEventId = Guid.NewGuid();
            var datalockEventFailureId1 = Guid.NewGuid();
            var datalockEventFailureId2 = Guid.NewGuid();
            var datalockEventFailureId3 = Guid.NewGuid();
            var datalockEventFailureId4 = Guid.NewGuid();

            await using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(sql, new
            {
                ukprn, uln, datalockEventId,
                datalockEventFailureId1, datalockEventFailureId2, datalockEventFailureId3, datalockEventFailureId4
            });
        }

        public async Task ClearLearner(long ukprn, long uln)
        {
            const string sql = @"
DELETE FROM Payments2.Job where Ukprn = @ukprn
DELETE Payments2.Apprenticeship WHERE Uln = @uln AND Ukprn = @ukprn;
DELETE Payments2.Apprenticeship WHERE Id = -123456;

DELETE Payments2.DatalockEventPayablePeriod
WHERE DatalockEventId IN (
    SELECT EventId 
    FROM Payments2.DatalockEvent
    WHERE LearnerUln = @uln
    AND Ukprn = @ukprn
)

DELETE Payments2.DataLockEventNonPayablePeriodFailures
WHERE DatalockEventNonPayablePeriodId IN (
	SELECT DatalockEventNonPayablePeriodId
	FROM Payments2.DataLockEventNonPayablePeriod
	WHERE DatalockEventId IN (
		SELECT EventId 
		FROM Payments2.DatalockEvent
		WHERE LearnerUln = @uln
		AND Ukprn = @ukprn
	)
)

DELETE Payments2.DataLockEventNonPayablePeriod
WHERE DatalockEventId IN (
	SELECT EventId 
	FROM Payments2.DatalockEvent
	WHERE LearnerUln = @uln
	AND Ukprn = @ukprn
)

DELETE Payments2.DatalockEventPriceEpisode
WHERE DatalockEventId IN (
	SELECT EventId 
	FROM Payments2.DatalockEvent
	WHERE LearnerUln = @uln
	AND Ukprn = @ukprn
)

DELETE Payments2.DatalockEvent
WHERE LearnerUln = @uln
AND Ukprn = @ukprn 
";

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new {ukprn, uln});
        }
    }
}

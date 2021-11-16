CREATE TABLE Payments2.MigrationStatus
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
	[Identifier] UNIQUEIDENTIFIER NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[Status] TINYINT NOT NULL,
	[CreationDate] DATETIMEOFFSET NOT NULL
)
GO

Create Unique Index UX_MigrationStatus_LogicalDuplicates on Payments2.MigrationStatus( [Ukprn] )
GO
CREATE TABLE [dbo].[LatestSubmissionJob]
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT PK_Id PRIMARY KEY CLUSTERED,
	DCJobId BIGINT NULL,
	Ukprn BIGINT NULL, 
	CollectionPeriod TINYINT NOT NULL,
	AcademicYear SMALLINT NOT NULL,
	IlrSubmissionDateTime DATETIME NULL,
	EventTime DATETIMEOFFSET NOT NULL,
	CreationDate DATETIMEOFFSET NOT NULL CONSTRAINT DF_Job__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)
GO

Create Unique Index UX_LatestSubmissionJob_LogicalDuplicates on [dbo].[LatestSubmissionJob] ( [DCJobId], [Ukprn], [AcademicYear], [CollectionPeriod] )
GO



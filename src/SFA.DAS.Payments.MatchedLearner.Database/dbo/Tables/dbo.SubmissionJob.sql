CREATE TABLE [dbo].[SubmissionJob]
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT PK_Id PRIMARY KEY CLUSTERED,
	DCJobId BIGINT NULL,
	Ukprn BIGINT NULL, 
	CollectionPeriod TINYINT NOT NULL,
	AcademicYear SMALLINT NOT NULL,
	IlrSubmissionDateTime DATETIME2 NULL,
	EventTime DATETIMEOFFSET NOT NULL,
	CreationDate DATETIMEOFFSET NOT NULL CONSTRAINT DF_Job__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)
GO

Create Unique Index UX_SubmissionJob_LogicalDuplicates on [dbo].[SubmissionJob] ( [DCJobId], [Ukprn], [AcademicYear], [CollectionPeriod], [IlrSubmissionDateTime] )
GO



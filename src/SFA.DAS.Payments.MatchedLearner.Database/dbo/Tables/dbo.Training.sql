﻿CREATE TABLE [dbo].[Training]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
	[EventTime] DATETIMEOFFSET NOT NULL,
	[EventId] UNIQUEIDENTIFIER NOT NULL,
	[IlrSubmissionDate] DATETIME2 NOT NULL, 
    [IlrSubmissionWindowPeriod] TINYINT NOT NULL,
	[AcademicYear] SMALLINT NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[Uln] BIGINT NOT NULL,
	[Reference] NVARCHAR(8) NOT NULL,
	[ProgrammeType] INT NOT NULL,
	[StandardCode] INT NOT NULL,
	[FrameworkCode] INT NOT NULL,
	[PathwayCode] INT NOT NULL,
	[FundingLineType] NVARCHAR(100) NOT NULL,
	[StartDate] DATETIME2 NOT NULL,
	[CompletionStatus] INT NULL,
	[CreationDate]  DATETIMEOFFSET NOT NULL CONSTRAINT DF_Training__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)
GO

Create Unique Index UX_Training_LogicalDuplicates on dbo.Training( [EventTime], [EventId], [IlrSubmissionDate], [IlrSubmissionWindowPeriod], [AcademicYear], [Ukprn], [Uln], [Reference], 
[ProgrammeType], [StandardCode], [FrameworkCode], [PathwayCode], [FundingLineType], [StartDate], [CompletionStatus] )
GO
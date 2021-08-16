CREATE TABLE [dbo].[Training]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
	[EventTime] DATETIMEOFFSET NOT NULL,
	[IlrSubmissionDate] DATETIMEOFFSET NOT NULL, 
    [IlrSubmissionWindowPeriod] INT NOT NULL,
	[AcademicYear] INT NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[Uln] BIGINT NOT NULL,
	[Reference] NVARCHAR(8) NOT NULL,
	[ProgrammeType] INT NOT NULL,
	[StandardCode] INT NOT NULL,
	[FrameworkCode] INT NOT NULL,
	[PathwayCode] INT NOT NULL,
	[FundingLineType] NVARCHAR(100) NOT NULL,
	[StartDate] DATETIME NOT NULL,
	[CreationDate]  DATETIMEOFFSET NOT NULL CONSTRAINT DF_Training__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)

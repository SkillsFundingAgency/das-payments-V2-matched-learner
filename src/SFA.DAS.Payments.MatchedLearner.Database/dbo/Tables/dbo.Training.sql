CREATE TABLE [dbo].[Training]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [StartDate] DATETIMEOFFSET NOT NULL,
	[EventTime] DATETIMEOFFSET NOT NULL,
	[IlrSubmissionDate] DATETIMEOFFSET NOT NULL, 
    [IlrSubmissionWindowPeriod] INT NOT NULL,
	[AcademicYear] INT NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[Uln] BIGINT NOT NULL,
	[Reference] NVARCHAR(50) NOT NULL,
	[ProgrammeType] INT NOT NULL,
	[StandardCode] INT NOT NULL,
	[FrameworkCode] INT NOT NULL,
	[PathwayCode] INT NOT NULL,
	[FundingLineType] NVARCHAR(100) NOT NULL,
	[TrainingStartDate] DATETIME NOT NULL
)

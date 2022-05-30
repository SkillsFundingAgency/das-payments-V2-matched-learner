CREATE TABLE Payments2.MigrationRunAttempt
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT PK_MigrationRunAttempt PRIMARY KEY CLUSTERED,
	[MigrationRunId] UNIQUEIDENTIFIER NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[Status] TINYINT NOT NULL,
	[TrainingCount] INT NOT NULL,
    [BatchNumber] INT NULL, 
    [TotalBatches] INT NULL, 
    [BatchUlns] NVARCHAR(MAX) NULL,
	[CompletionTime] DATETIMEOFFSET NULL,
	[CreationDate] DATETIMEOFFSET NOT NULL CONSTRAINT DF_MigrationRunAttempt__CreationDate DEFAULT (SYSDATETIMEOFFSET()), 
)
GO
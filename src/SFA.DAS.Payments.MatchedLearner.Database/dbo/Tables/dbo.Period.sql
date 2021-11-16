CREATE TABLE [dbo].[Period]
(
	[Id] BIGINT NOT NULL PRIMARY KEY,
    [PriceEpisodeId] BIGINT NOT NULL CONSTRAINT FK__Period_PriceEpisode FOREIGN KEY REFERENCES [dbo].[PriceEpisode] (Id),
    [IsPayable] BIT NOT NULL,
    [TransactionType] TINYINT NOT NULL, 
    [Period] TINYINT NOT NULL, 
    [Amount] DECIMAL(15, 5) NOT NULL, 
    [ApprenticeshipId] BIGINT NULL, 
    [AccountId] BIGINT NULL,
    [TransferSenderAccountId] BIGINT NOT NULL, 
    [ApprenticeshipEmployerType] TINYINT NULL, 
    [FailedDataLock1] BIT NOT NULL DEFAULT 0,
    [FailedDataLock2] BIT NOT NULL DEFAULT 0,
    [FailedDataLock3] BIT NOT NULL DEFAULT 0,
    [FailedDataLock4] BIT NOT NULL DEFAULT 0,
    [FailedDataLock5] BIT NOT NULL DEFAULT 0,
    [FailedDataLock6] BIT NOT NULL DEFAULT 0,
    [FailedDataLock7] BIT NOT NULL DEFAULT 0,
    [FailedDataLock8] BIT NOT NULL DEFAULT 0,
    [FailedDataLock9] BIT NOT NULL DEFAULT 0,
    [FailedDataLock10] BIT NOT NULL DEFAULT 0,
    [FailedDataLock11] BIT NOT NULL DEFAULT 0,
    [FailedDataLock12] BIT NOT NULL DEFAULT 0,
    [CreationDate]  DATETIMEOFFSET NOT NULL CONSTRAINT DF_Period__CreationDate DEFAULT (SYSDATETIMEOFFSET()), 
)
GO

Create Unique Index UX_Period_LogicalDuplicates on dbo.Period( [PriceEpisodeId], [IsPayable], [TransactionType], [Period], [Amount], [ApprenticeshipId], [AccountId], [TransferSenderAccountId], [ApprenticeshipEmployerType], 
[FailedDataLock1] , [FailedDataLock2] ,[FailedDataLock3] , [FailedDataLock4] , [FailedDataLock5] , [FailedDataLock6] , [FailedDataLock7] , [FailedDataLock8] , [FailedDataLock9] , [FailedDataLock10], [FailedDataLock11], [FailedDataLock12] )
GO
CREATE TABLE [dbo].[Period]
(
	[Id] BIGINT NOT NULL PRIMARY KEY,
    [PriceEpisodeId] BIGINT NOT NULL CONSTRAINT FK__Period_PriceEpisode FOREIGN KEY REFERENCES [dbo].[PriceEpisode] (Id),
    [Period] INT NOT NULL, 
    [IsPayable] BIT NOT NULL, 
    [AccountId] BIGINT NOT NULL, 
    [ApprenticeshipId] BIGINT NULL, 
    [ApprenticeshipEmployerType] INT NOT NULL, 
    [TransferSenderAccountId] BIGINT NOT NULL, 
    [DataLockFailures] NVARCHAR(27) NULL,
    [CreationDate]  DATETIMEOFFSET NOT NULL CONSTRAINT DF_Period__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)

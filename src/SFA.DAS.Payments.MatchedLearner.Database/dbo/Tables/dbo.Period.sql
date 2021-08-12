CREATE TABLE [dbo].[Period]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Period] INT NOT NULL, 
    [IsPayable] BIT NOT NULL, 
    [AccountId] BIGINT NOT NULL, 
    [ApprenticeshipId] BIGINT NULL, 
    [ApprenticeshipEmployerType] INT NOT NULL, 
    [TransferSenderAccountId] BIGINT NOT NULL, 
    [DataLockFailures] NVARCHAR(27) NULL
)

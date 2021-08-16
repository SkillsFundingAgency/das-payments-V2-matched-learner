CREATE TABLE [dbo].[PriceEpisode]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
    [TrainingId] BIGINT NOT NULL CONSTRAINT FK_PriceEpisode__Training FOREIGN KEY REFERENCES [dbo].[Training] (Id),
    [Identifier] NVARCHAR(50) NOT NULL, 
    [AcademicYear] SMALLINT NOT NULL, 
    [CollectionPeriod] TINYINT NOT NULL, 
    [AgreedPrice] DECIMAL(15, 5) NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NULL, 
    [NumberOfInstalments] INT NOT NULL, 
    [InstalmentAmount] DECIMAL(15, 5) NOT NULL, 
    [CompletionAmount] DECIMAL(15, 5) NOT NULL, 
    [TotalNegotiatedPriceStartDate] DATETIME NULL,
    [CreationDate]  DATETIMEOFFSET NOT NULL CONSTRAINT DF_PriceEpisode__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)

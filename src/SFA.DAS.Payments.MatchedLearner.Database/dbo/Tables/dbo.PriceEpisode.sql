CREATE TABLE [dbo].[PriceEpisode]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
    [TrainingId] BIGINT NOT NULL CONSTRAINT FK_PriceEpisode__Training FOREIGN KEY REFERENCES [dbo].[Training] (Id),
    [Identifier] NVARCHAR(10) NOT NULL, 
    [AcademicYear] SMALLINT NOT NULL, 
    [CollectionPeriod] TINYINT NOT NULL, 
    [AgreedPrice] DECIMAL NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NULL, 
    [NumberOfInstalments] INT NOT NULL, 
    [InstalmentAmount] DECIMAL NOT NULL, 
    [CompletionAmount] DECIMAL NOT NULL, 
    [TotalNegotiatedPriceStartDate] DATETIME NULL,
    [CreationDate]  DATETIMEOFFSET NOT NULL CONSTRAINT DF_PriceEpisode__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)

CREATE TABLE [dbo].[PriceEpisode]
(
    [Id] BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT PK_PriceEpisode PRIMARY KEY CLUSTERED, 
    [TrainingId] BIGINT NOT NULL CONSTRAINT FK_PriceEpisode__Training FOREIGN KEY REFERENCES [dbo].[Training] (Id) ON DELETE CASCADE,
    [Identifier] NVARCHAR(50) NOT NULL, 
    [AcademicYear] SMALLINT NOT NULL, 
    [CollectionPeriod] TINYINT NOT NULL, 
    [AgreedPrice] DECIMAL(15, 5) NOT NULL, 
    [StartDate] DATETIME2 NOT NULL, 
    [ActualEndDate] DATETIME2 NULL, 
    [PlannedEndDate] DATETIME2 NOT NULL, 
    [NumberOfInstalments] INT NOT NULL, 
    [InstalmentAmount] DECIMAL(15, 5) NOT NULL, 
    [CompletionAmount] DECIMAL(15, 5) NOT NULL, 
    [TotalNegotiatedPriceStartDate] DATETIME2 NULL,
    [CreationDate]  DATETIMEOFFSET NOT NULL CONSTRAINT DF_PriceEpisode__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
)
GO

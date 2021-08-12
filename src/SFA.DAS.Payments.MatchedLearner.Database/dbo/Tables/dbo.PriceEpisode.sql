CREATE TABLE [dbo].[PriceEpisode]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Identifier] NVARCHAR(10) NOT NULL, 
    [AcademicYear] SMALLINT NOT NULL, 
    [CollectionPeriod] TINYINT NOT NULL, 
    [AgreedPrice] DECIMAL NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NULL, 
    [NumberOfInstalments] INT NOT NULL, 
    [InstalmentAmount] DECIMAL NOT NULL, 
    [CompletionAmount] DECIMAL NOT NULL, 
    [TotalNegotiatedPriceStartDate] DATETIME NULL
)

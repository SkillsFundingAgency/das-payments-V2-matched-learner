﻿CREATE TABLE [Payments2].[DataLockEventNonPayablePeriod]
(
	Id BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT PK_DataLockEventNonPayablePeriod PRIMARY KEY CLUSTERED,	
	DataLockEventId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_DataLockEventNonPayablePeriod__DataLockEvent FOREIGN KEY REFERENCES [Payments2].[DataLockEvent] (EventId) ON DELETE CASCADE, 
	DataLockEventNonPayablePeriodId UNIQUEIDENTIFIER NOT NULL CONSTRAINT UQ_DataLockEventNonPayablePeriod__DataLockEventNonPayablePeriodId UNIQUE,
	PriceEpisodeIdentifier NVARCHAR(50) NULL,
	TransactionType TINYINT NOT NULL, 
	DeliveryPeriod TINYINT NOT NULL, 
	Amount DECIMAL(15,5) NOT NULL,
	SfaContributionPercentage DECIMAL(15,5) NULL,
	CreationDate DATETIMEOFFSET NOT NULL CONSTRAINT DF_DataLockEventNonPayablePeriod__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
	LearningStartDate DATETIME2 NULL,

)
GO

CREATE NONCLUSTERED INDEX [IX_DataLockEventNonPayablePeriod__DataLockEventId] ON [Payments2].[DataLockEventNonPayablePeriod] ([DataLockEventId], [TransactionType], [PriceEpisodeIdentifier], [Amount], [DeliveryPeriod]) WITH (ONLINE = ON);
GO
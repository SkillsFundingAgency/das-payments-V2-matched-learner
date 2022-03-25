CREATE TABLE [Payments2].[ApprenticeshipInput](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Ukprn] [bigint] NOT NULL,
	[Uln] [bigint] NOT NULL,
	[CreationDate] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_ApprenticeshipInput] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Payments2].[ApprenticeshipInput] ADD  CONSTRAINT [DF_ApprenticeshipInput__CreationDate]  DEFAULT (sysdatetimeoffset()) FOR [CreationDate]
GO


CREATE TABLE [Payments2].[ApprenticeshipOutPut](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Ukprn] [bigint] NOT NULL,
	[Uln] [bigint] NOT NULL,
	[LearnerJson] [nvarchar](max) NOT NULL,
	[CreationDate] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_ApprenticeshipOutPut] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Payments2].[ApprenticeshipOutPut] ADD  CONSTRAINT [DF_ApprenticeshipOutPut__CreationDate]  DEFAULT (sysdatetimeoffset()) FOR [CreationDate]
GO



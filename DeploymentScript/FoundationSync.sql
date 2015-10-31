USE [FoundationSync]
GO
/****** Object:  Table [dbo].[Databases]    Script Date: 10/31/2015 2:06:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Databases](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[LastStarted] [datetime2](7) NULL,
	[LastUpdated] [datetime2](7) NULL,
 CONSTRAINT [PK_Databases] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserProfiles]    Script Date: 10/31/2015 2:06:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UserProfiles](
	[NTName] [nvarchar](20) NULL,
	[ObjectSID] [varbinary](512) NOT NULL,
	[PreferredName] [nvarchar](256) NULL,
	[PictureUrl] [nvarchar](max) NULL,
	[Email] [nvarchar](256) NULL,
	[SipAddress] [nchar](10) NULL,
	[Properties] [xml] COLUMN_SET FOR ALL_SPARSE_COLUMNS  NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO

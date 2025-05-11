USE [MoviesMadeEasyDB];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

----------------------------------------------------------
-- 1. Create Title Table
----------------------------------------------------------
CREATE TABLE [dbo].[Title] (
    [id]                 INT IDENTITY(1,1) NOT NULL,
    [title_name]         NVARCHAR(255)    NOT NULL,
    [year]               INT              NOT NULL,
    [poster_url]         NVARCHAR(MAX)     NULL,
    [genres]             NVARCHAR(MAX)     NULL,
    [rating]             NVARCHAR(50)      NULL,
    [overview]           NVARCHAR(MAX)     NULL,
    [streaming_services] NVARCHAR(MAX)     NULL,
    [last_updated]       DATETIME         NOT NULL CONSTRAINT [DF_Title_last_updated] DEFAULT (GETDATE()),
    CONSTRAINT [PK_Title] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

----------------------------------------------------------
-- 2. Create StreamingService Table
----------------------------------------------------------
CREATE TABLE [dbo].[StreamingService] (
    [id]       INT IDENTITY(1,1) PRIMARY KEY,
    [name]     NVARCHAR(255)    NOT NULL,
    [region]   NVARCHAR(50)     NULL,
    [base_url] NVARCHAR(MAX)    NULL,
    [logo_url] NVARCHAR(MAX)    NULL
);
GO

----------------------------------------------------------
-- 3. Create User Table (Non-Auth Data)
----------------------------------------------------------
CREATE TABLE [dbo].[User] (
    [Id]                   INT             IDENTITY(1,1) NOT NULL,
    [AspNetUserId]         NVARCHAR(450)   NOT NULL,
    [FirstName]            NVARCHAR(MAX)   NOT NULL,
    [LastName]             NVARCHAR(MAX)   NOT NULL,
    [ColorMode]            NVARCHAR(MAX)   NOT NULL CONSTRAINT [DF_User_ColorMode] DEFAULT (N''),
    [FontSize]             NVARCHAR(MAX)   NOT NULL CONSTRAINT [DF_User_FontSize] DEFAULT (N''),
    [FontType]             NVARCHAR(MAX)   NOT NULL CONSTRAINT [DF_User_FontType] DEFAULT (N''),
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

----------------------------------------------------------
-- 4. Create UserStreamingServices Join Table
----------------------------------------------------------
CREATE TABLE [dbo].[UserStreamingServices] (
    [UserId]             INT NOT NULL, 
    [StreamingServiceId] INT NOT NULL,
    CONSTRAINT [PK_UserStreamingServices] PRIMARY KEY CLUSTERED ([UserId] ASC, [StreamingServiceId] ASC)
);
GO

ALTER TABLE [dbo].[UserStreamingServices]
    WITH CHECK ADD CONSTRAINT [FK_UserStreamingServices_User_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[User] ([Id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[UserStreamingServices]
    WITH CHECK ADD CONSTRAINT [FK_UserStreamingServices_StreamingService_StreamingServiceId]
    FOREIGN KEY ([StreamingServiceId])
    REFERENCES [dbo].[StreamingService] ([id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[UserStreamingServices]
    ADD [MonthlyCost] DECIMAL(18,2) NULL;
GO

----------------------------------------------------------
-- 5. Create RecentlyViewedTitles Table
----------------------------------------------------------
CREATE TABLE [dbo].[RecentlyViewedTitles] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NOT NULL,
    [TitleId] INT NOT NULL,
    [ViewedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_RecentlyViewedTitles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecentlyViewedTitles_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecentlyViewedTitles_Title_TitleId] FOREIGN KEY ([TitleId]) REFERENCES [dbo].[Title]([id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_User_Title] UNIQUE (UserId, TitleId)
);
GO

USE [MoviesMadeEasyDB];
GO

----------------------------------------------------------
-- 1. Create Title Table
----------------------------------------------------------
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [dbo].[Title] (
    [id]           UNIQUEIDENTIFIER NOT NULL,
    [external_id]  NVARCHAR(255)    NULL,
    [title_name]   NVARCHAR(255)    NOT NULL,
    [year]         INT              NOT NULL,
    [type]         NVARCHAR(50)     NOT NULL,
    [last_updated] DATETIME         NOT NULL CONSTRAINT [DF_Title_last_updated] DEFAULT (GETDATE()),
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
-- 3. Create Custom User Table (Non‑Auth Data)
----------------------------------------------------------

CREATE TABLE [dbo].[User] (
    [Id]                   INT             IDENTITY(1,1) NOT NULL,
    [AspNetUserId]        NVARCHAR(450)   NOT NULL,   -- Link to ASP.NET Identity table
    [FirstName]            NVARCHAR(MAX)   NOT NULL,
    [LastName]             NVARCHAR(MAX)   NOT NULL,
    [RecentlyViewedShowId] UNIQUEIDENTIFIER NULL,
    [ColorMode]            NVARCHAR(MAX)   NOT NULL CONSTRAINT [DF_User_ColorMode] DEFAULT (N''),
    [FontSize]             NVARCHAR(MAX)   NOT NULL CONSTRAINT [DF_User_FontSize] DEFAULT (N''),
    [FontType]             NVARCHAR(MAX)   NOT NULL CONSTRAINT [DF_User_FontType] DEFAULT (N''),
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Foreign key linking RecentlyViewedShowId to Title table
ALTER TABLE [dbo].[User]
    WITH CHECK ADD CONSTRAINT [FK_User_Title_RecentlyViewedShowId]
    FOREIGN KEY ([RecentlyViewedShowId])
    REFERENCES [dbo].[Title] ([id]);
GO

ALTER TABLE [dbo].[User]
    CHECK CONSTRAINT [FK_User_Title_RecentlyViewedShowId];
GO

----------------------------------------------------------
-- 4. Create UserStreamingServices Join Table
----------------------------------------------------------

CREATE TABLE [dbo].[UserStreamingServices] (
    [UserId]              INT              NOT NULL, 
    [StreamingServiceId]  INT              NOT NULL,
 CONSTRAINT [PK_UserStreamingServices] PRIMARY KEY CLUSTERED ([UserId] ASC, [StreamingServiceId] ASC)
);
GO

-- Foreign key linking UserStreamingServices to User table
ALTER TABLE [dbo].[UserStreamingServices]
    WITH CHECK ADD CONSTRAINT [FK_UserStreamingServices_User_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[User] ([Id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[UserStreamingServices]
    CHECK CONSTRAINT [FK_UserStreamingServices_User_UserId];
GO

-- Foreign key linking UserStreamingServices to StreamingService table
ALTER TABLE [dbo].[UserStreamingServices]
    WITH CHECK ADD CONSTRAINT [FK_UserStreamingServices_StreamingService_StreamingServiceId]
    FOREIGN KEY ([StreamingServiceId])
    REFERENCES [dbo].[StreamingService] ([id])
    ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[UserStreamingServices]
    CHECK CONSTRAINT [FK_UserStreamingServices_StreamingService_StreamingServiceId];
GO


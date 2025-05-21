USE [MoviesMadeEasyDB];
GO

IF OBJECT_ID('dbo.RecentlyViewedTitles', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[RecentlyViewedTitles];
END
GO

IF OBJECT_ID('dbo.UserStreamingServices', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[UserStreamingServices];
END
GO

IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[User];
END
GO

IF OBJECT_ID('dbo.StreamingService', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[StreamingService];
END
GO

IF OBJECT_ID('dbo.Title', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Title];
END
GO

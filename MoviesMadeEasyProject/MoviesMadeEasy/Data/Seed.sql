USE [MoviesMadeEasyDB];
GO

INSERT INTO [dbo].[StreamingService] ([name], [region], [base_url], [logo_url], [monthly_price])
VALUES 
    ('Netflix', 'US', 'https://www.netflix.com/login', '/images/Netflix_Symbol_RGB.png', NULL),
    ('Hulu', 'US', 'https://auth.hulu.com/web/login', '/images/hulu-Green-digital.png', NULL),
    ('Disney+', 'US', 'https://www.disneyplus.com/login', '/images/disney_logo_march_2024_050fef2e.png', NULL),
    ('Amazon Prime Video', 'US', 'https://www.primevideo.com', '/images/AmazonPrimeVideo.png', NULL),
    ('Max "HBO Max"', 'US', 'https://play.max.com/sign-in', '/images/maxlogo.jpg', NULL),
    ('Apple TV+', 'US', 'https://tv.apple.com/login', '/images/AppleTV-iOS.png', NULL),
    ('Peacock', 'US', 'https://www.peacocktv.com/signin', '/images/Peacock_P.png', NULL),
    ('Paramount+', 'US', 'https://www.paramountplus.com/account/signin/', '/images/Paramountplus.png', NULL),
    ('Starz', 'US', 'https://www.starz.com/login', '/images/Starz_Prism_Button_Option_01.png', NULL),
    ('Tubi', 'US', 'https://tubitv.com/login', '/images/tubitvlogo.png', NULL),
    ('Pluto TV', 'US', 'https://pluto.tv/en/login', '/images/Pluto-TV-Logo.jpg', NULL),
    ('BritBox', 'US', 'https://www.britbox.com/us/', '/images/britboxlogo.png', NULL),
    ('AMC+', 'US', 'https://www.amcplus.com/login', '/images/amcpluslogo.png', NULL);
GO

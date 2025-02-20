using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;
using System;

namespace MoviesMadeEasy.Data
{
    public static class StreamingSeedData
    {
        public static void SeedStreamingServices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StreamingService>().HasData(
                new StreamingService 
                { 
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"), 
                    Name = "Netflix", 
                    Region = "US", 
                    BaseUrl = "https://www.netflix.com/login", 
                    LogoUrl = "/images/Netflix_Symbol_RGB.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), 
                    Name = "Hulu", 
                    Region = "US", 
                    BaseUrl = "https://auth.hulu.com/web/login", 
                    LogoUrl = "/images/hulu-Green-digital.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("9b5f8e4c-a3c6-4856-a1d2-e50e133b5f61"), 
                    Name = "Disney+", 
                    Region = "US", 
                    BaseUrl = "https://www.disneyplus.com/login", 
                    LogoUrl = "/images/disney_logo_march_2024_050fef2e.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("8b0a7e6e-6b2a-4c85-b0e7-763c1e9f3bfb"), 
                    Name = "Amazon Prime Video", 
                    Region = "US", 
                    BaseUrl = "https://www.amazon.com/ap/signin", 
                    LogoUrl = "/images/AmazonPrimeVideo.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("2d68f375-6e3b-46b3-b67a-d1629b6fef6b"), 
                    Name = "Max (formerly HBO Max)", 
                    Region = "US", 
                    BaseUrl = "https://play.max.com/sign-in", 
                    LogoUrl = "/images/maxlogo.jpg" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("b9bdb87b-7933-4e44-9cd3-4e2bdee2eaea"), 
                    Name = "Apple TV+", 
                    Region = "US", 
                    BaseUrl = "https://tv.apple.com/login", 
                    LogoUrl = "/images/AppleTV-iOS.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("135f363b-d38f-4c67-befb-b2f57b1c7ef5"), 
                    Name = "Peacock", 
                    Region = "US", 
                    BaseUrl = "https://www.peacocktv.com/signin", 
                    LogoUrl = "/images/Peacock_'P'.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("217d4328-2c0a-4c90-a348-6a82c9bb5734"), 
                    Name = "Paramount+", 
                    Region = "US", 
                    BaseUrl = "https://www.paramountplus.com/account/signin/", 
                    LogoUrl = "/images/Paramountplus.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("bc6b7342-8e3a-4943-b36e-408e882a4dbf"), 
                    Name = "Starz", 
                    Region = "US", 
                    BaseUrl = "https://www.starz.com/login", 
                    LogoUrl = "/images/Starz_Prism_Button_Option_01.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("cb4194a3-4826-4bc7-bd1d-b2466c7915cf"), 
                    Name = "Tubi", 
                    Region = "US", 
                    BaseUrl = "https://tubitv.com/login", 
                    LogoUrl = "/images/tubitlogo.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("5b44db3d-8f5f-49b2-99b4-3c674fc4a7b8"), 
                    Name = "Pluto TV", 
                    Region = "US", 
                    BaseUrl = "https://pluto.tv/en/login", 
                    LogoUrl = "/images/Pluto-TV-Logo.jpg" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("9cf82c66-4a3b-4e82-bf37-34d6b8f5d546"), 
                    Name = "BritBox", 
                    Region = "US", 
                    BaseUrl = "https://www.britbox.com/us/account/signin", 
                    LogoUrl = "/images/britboxlogo.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("7e3c946d-ffeb-4c02-abe8-5df8cd7f536f"), 
                    Name = "AMC+", 
                    Region = "US", 
                    BaseUrl = "https://www.amcplus.com/login", 
                    LogoUrl = "/images/amcpluslogo.png" 
                }
            );

        }
    }
}

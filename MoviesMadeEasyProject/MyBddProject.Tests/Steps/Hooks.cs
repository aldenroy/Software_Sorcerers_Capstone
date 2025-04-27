using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using MoviesMadeEasy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MoviesMadeEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IWebDriver? _driver;
        private readonly IConfiguration _configuration;
        private TestWebApplicationFactory _factory;
        private IServiceScope _serviceScope;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Set environment variable to indicate we're in a test environment
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            try
            {
                // Create the factory and scope
                _factory = new TestWebApplicationFactory();
                _serviceScope = _factory.Services.CreateScope();

                // Seed test data
                SeedTestData();

                // Start the server and get a client
                var client = _factory.CreateClient();

                // Setup ChromeDriver
                var options = new ChromeOptions();
                options.AddArguments("--headless", "--disable-gpu");

                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
                {
                    options.AddArguments(
                        "--no-sandbox",
                        "--disable-dev-shm-usage",
                        "--window-size=1920,1080"
                    );
                }

                _driver = new ChromeDriver(options);

                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
                {
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
                    _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                }

                _objectContainer.RegisterInstanceAs(_driver);

                // Navigate to the application
                var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                _driver.Navigate().GoToUrl(baseUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SETUP ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (_driver != null)
            {
                try
                {
                    _driver.Quit();
                    _driver.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error quitting driver: {ex.Message}");
                }
            }

            _serviceScope?.Dispose();
            _factory?.Dispose();
        }

        private void SeedTestData()
        {
            var userDbContext = _serviceScope.ServiceProvider.GetRequiredService<UserDbContext>();
            var userManager = _serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Ensure database is created
            userDbContext.Database.EnsureCreated();

            // Clear existing data
            userDbContext.RecentlyViewedTitles.RemoveRange(userDbContext.RecentlyViewedTitles);
            userDbContext.UserStreamingServices.RemoveRange(userDbContext.UserStreamingServices);
            userDbContext.Titles.RemoveRange(userDbContext.Titles);
            userDbContext.Users.RemoveRange(userDbContext.Users);
            userDbContext.StreamingServices.RemoveRange(userDbContext.StreamingServices);
            userDbContext.SaveChanges();

            // Seed streaming services
            var streamingServices = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix", Region = "US", BaseUrl = "https://www.netflix.com/login", LogoUrl = "/images/Netflix_Symbol_RGB.png" },
                new StreamingService { Name = "Hulu", Region = "US", BaseUrl = "https://auth.hulu.com/web/login", LogoUrl = "/images/hulu-Green-digital.png" },
                new StreamingService { Name = "Disney+", Region = "US", BaseUrl = "https://www.disneyplus.com/login", LogoUrl = "/images/disney_logo_march_2024_050fef2e.png" },
                new StreamingService { Name = "Amazon Prime Video", Region = "US", BaseUrl = "https://www.primevideo.com", LogoUrl = "/images/AmazonPrimeVideo.png" },
                new StreamingService { Name = "Max \"HBO Max\"", Region = "US", BaseUrl = "https://play.max.com/sign-in", LogoUrl = "/images/maxlogo.jpg" },
                new StreamingService { Name = "Apple TV+", Region = "US", BaseUrl = "https://tv.apple.com/login", LogoUrl = "/images/AppleTV-iOS.png" }
            };

            foreach (var service in streamingServices)
            {
                userDbContext.StreamingServices.Add(service);
            }
            userDbContext.SaveChanges();

            // Seed movies
            var pokemonMovie = new Title
            {
                TitleName = "Pokemon 4Ever",
                Year = 2001,
                PosterUrl = "https://example.com/pokemon4ever.jpg",
                Genres = "Animation,Adventure",
                Rating = "5.8",
                Overview = "Ash and friends must save a Celebi from a hunter and a corrupted future.",
                StreamingServices = "Hulu,Disney+",
                LastUpdated = DateTime.UtcNow.AddDays(-1)
            };
            userDbContext.Titles.Add(pokemonMovie);

            var herMovie = new Title
            {
                TitleName = "Her",
                Year = 2013,
                PosterUrl = "https://example.com/her.jpg",
                Genres = "Romance,Drama,Sci-Fi",
                Rating = "8.0",
                Overview = "In a near future, a lonely writer develops an unlikely relationship with an operating system.",
                StreamingServices = "Netflix",
                LastUpdated = DateTime.UtcNow.AddDays(-1)
            };
            userDbContext.Titles.Add(herMovie);
            userDbContext.SaveChanges();

            int pokemonId = pokemonMovie.Id;
            int herId = herMovie.Id;

            // Create test users
            var userId = SeedUser(userManager, userDbContext, "testuser@example.com", "Ab+1234");
            var userId2 = SeedUser(userManager, userDbContext, "testuser2@example.com", "Ab+1234");

            // Add subscriptions
            var huluService = userDbContext.StreamingServices.FirstOrDefault(s => s.Name == "Hulu");
            if (huluService != null)
            {
                userDbContext.UserStreamingServices.Add(new UserStreamingService
                {
                    UserId = userId,
                    StreamingServiceId = huluService.Id
                });

                userDbContext.UserStreamingServices.Add(new UserStreamingService
                {
                    UserId = userId2,
                    StreamingServiceId = huluService.Id
                });

                userDbContext.SaveChanges();
            }

            // Add recently viewed titles
            userDbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
            {
                UserId = userId,
                TitleId = pokemonId,
                ViewedAt = DateTime.UtcNow.AddDays(-30)
            });

            userDbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
            {
                UserId = userId,
                TitleId = herId,
                ViewedAt = DateTime.UtcNow
            });

            userDbContext.SaveChanges();
        }

        private int SeedUser(UserManager<IdentityUser> userManager, UserDbContext userDbContext,
            string email, string password)
        {
            // Create identity user
            var testUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var existingUser = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
            if (existingUser != null)
            {
                userManager.DeleteAsync(existingUser).GetAwaiter().GetResult();
            }

            var result = userManager.CreateAsync(testUser, password).GetAwaiter().GetResult();

            // Create custom user
            var customUser = new User
            {
                AspNetUserId = testUser.Id,
                FirstName = "Test",
                LastName = "User",
                ColorMode = "Light",
                FontSize = "Medium",
                FontType = "Standard"
            };
            userDbContext.Users.Add(customUser);
            userDbContext.SaveChanges();

            return customUser.Id;
        }
    }
}
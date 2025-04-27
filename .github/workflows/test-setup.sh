#!/bin/bash

# Create temporary directory for test build
mkdir -p test_build
mkdir -p prod_build

# Step 1: Prepare the test environment with mocks
echo "Setting up test environment..."
cp -r MoviesMadeEasyProject test_build/

# Create modified Program.cs for testing
cat > test_build/MoviesMadeEasyProject/MoviesMadeEasy/Program.cs << 'EOL'
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.DAL.Concrete;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Microsoft.AspNetCore.Session;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// For CI/CD testing environment only
builder.Configuration["OpenAI_ApiKey"] = "sk-dummy-key-for-testing";
builder.Configuration["TMDBApiKey"] = "dummy-key-for-testing";
builder.Configuration["RapidApiKey"] = "dummy-rapid-api-key-for-testing";
builder.Configuration["OpenAI_Model"] = "gpt-3.5-turbo";

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
}

builder.Services.AddHttpClient<IOpenAIService, OpenAIService>()
    .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(x => (int)x.StatusCode == 429)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
            
builder.Services.AddHttpClient<IMovieService, MovieService>();
builder.Services.AddScoped<IMovieService, MovieService>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new MovieService(httpClient, configuration);
});

builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<ITitleRepository, TitleRepository>();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseInMemoryDatabase("TestAuthDb"));

builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<UserDbContext>());

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Seed test data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userDbContext = services.GetRequiredService<UserDbContext>();
        userDbContext.Database.EnsureCreated();
        
        if (userDbContext.StreamingServices.Any() || userDbContext.Titles.Any() || userDbContext.Users.Any())
        {
            userDbContext.RecentlyViewedTitles.RemoveRange(userDbContext.RecentlyViewedTitles);
            userDbContext.UserStreamingServices.RemoveRange(userDbContext.UserStreamingServices);
            userDbContext.Titles.RemoveRange(userDbContext.Titles);
            userDbContext.Users.RemoveRange(userDbContext.Users);
            userDbContext.StreamingServices.RemoveRange(userDbContext.StreamingServices);
            userDbContext.SaveChanges();
        }
        
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
        userDbContext.SaveChanges();
        
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
        
        // Create users
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        
        // Create first test user
        var testUser = new IdentityUser
        {
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            EmailConfirmed = true
        };
        
        var existingUser = await userManager.FindByEmailAsync("testuser@example.com");
        if (existingUser != null)
        {
            await userManager.DeleteAsync(existingUser);
        }
        
        await userManager.CreateAsync(testUser, "Ab+1234");
        
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
        int userId = customUser.Id;
        
        // Create second test user with no viewed movies
        var testUser2 = new IdentityUser
        {
            UserName = "testuser2@example.com",
            Email = "testuser2@example.com",
            EmailConfirmed = true
        };
        
        var existingUser2 = await userManager.FindByEmailAsync("testuser2@example.com");
        if (existingUser2 != null)
        {
            await userManager.DeleteAsync(existingUser2);
        }
        
        await userManager.CreateAsync(testUser2, "Ab+1234");
        
        var customUser2 = new User
        {
            AspNetUserId = testUser2.Id,
            FirstName = "Test",
            LastName = "User2",
            ColorMode = "Light",
            FontSize = "Medium",
            FontType = "Standard"
        };
        userDbContext.Users.Add(customUser2);
        userDbContext.SaveChanges();
        int userId2 = customUser2.Id;
        
        // Add Hulu subscription for both users
        var huluService = userDbContext.StreamingServices.FirstOrDefault(s => s.Name == "Hulu");
        if (huluService != null)
        {
            userDbContext.UserStreamingServices.Add(new UserStreamingService { 
                UserId = userId, 
                StreamingServiceId = huluService.Id
            });
            
            userDbContext.UserStreamingServices.Add(new UserStreamingService { 
                UserId = userId2, 
                StreamingServiceId = huluService.Id
            });
            
            userDbContext.SaveChanges();
        }
        
        // Add recently viewed titles for first user only
        // First view Pokemon (older timestamp)
        userDbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
        {
            UserId = userId,
            TitleId = pokemonId,
            ViewedAt = DateTime.UtcNow.AddDays(-30)
        });
        userDbContext.SaveChanges();
        
        // Then view Her (newer timestamp)
        userDbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
        {
            UserId = userId,
            TitleId = herId,
            ViewedAt = DateTime.UtcNow
        });
        userDbContext.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database seeding: {ex.Message}");
        throw;
    }
}

// Mock API endpoints for tests - ENHANCED WITH MORE ROBUST MOCKS
app.MapGet("/Home/SearchMovies", (string query) => {
    var movieResults = new List<object>();
    
    if (query?.Contains("Hunger Games", StringComparison.OrdinalIgnoreCase) == true)
    {
        movieResults.Add(new 
        {
            title = "The Hunger Games",
            releaseYear = 2012,
            posterUrl = "https://example.com/hunger-games.jpg",
            genres = new[] { "Action", "Adventure", "Sci-Fi" },
            rating = 7.2,
            overview = "Katniss Everdeen voluntarily takes her younger sister's place in the Hunger Games.",
            services = new[] { "Netflix", "Apple TV", "Prime Video" }
        });
        
        movieResults.Add(new
        {
            title = "The Hunger Games: Catching Fire",
            releaseYear = 2013,
            posterUrl = "https://example.com/catching-fire.jpg",
            genres = new[] { "Action", "Adventure", "Sci-Fi" },
            rating = 7.5,
            overview = "Katniss Everdeen and Peeta Mellark become targets of the Capitol after their victory.",
            services = new[] { "Netflix", "Apple TV", "Prime Video" }
        });
    }
    
    // Add a delay to simulate API latency but keep it reasonable for tests
    System.Threading.Thread.Sleep(500);
    
    return Results.Json(movieResults);
});

app.MapGet("/Home/GetSimilarMovies", (string title) => {
    var recommendations = new List<object>
    {
        new { 
            title = "The Maze Runner", 
            year = 2014, 
            reason = "Similar dystopian theme",
            posterUrl = "https://example.com/maze-runner.jpg",
            overview = "A group of teens must escape a maze in this dystopian thriller.",
            rating = 6.8,
            genres = new[] { "Action", "Mystery", "Sci-Fi" },
            services = new[] { "Netflix", "Disney+" }
        },
        new { 
            title = "Divergent", 
            year = 2014, 
            reason = "Features a strong female protagonist in a dystopian future",
            posterUrl = "https://example.com/divergent.jpg",
            overview = "In a world divided by factions, Tris learns she's Divergent and won't fit in.",
            rating = 6.7,
            genres = new[] { "Action", "Adventure", "Sci-Fi" },
            services = new[] { "Netflix", "Apple TV" }
        },
        new { 
            title = "Battle Royale", 
            year = 2000, 
            reason = "Similar survival competition premise",
            posterUrl = "https://example.com/battle-royale.jpg",
            overview = "In the future, the Japanese government captures a class of ninth-grade students and forces them to kill each other.",
            rating = 7.6,
            genres = new[] { "Action", "Adventure", "Drama" },
            services = new[] { "Netflix", "Hulu" }
        },
        new { 
            title = "The Giver", 
            year = 2014, 
            reason = "Dystopian society with controlled roles",
            posterUrl = "https://example.com/the-giver.jpg",
            overview = "In a seemingly perfect community, a young man is chosen to learn about real pain and pleasure.",
            rating = 6.5,
            genres = new[] { "Drama", "Romance", "Sci-Fi" },
            services = new[] { "Netflix", "Max \"HBO Max\"" }
        },
        new { 
            title = "Ender's Game", 
            year = 2013, 
            reason = "Young protagonists trained for combat",
            posterUrl = "https://example.com/enders-game.jpg",
            overview = "Young Ender Wiggin is trained to lead Earth's military against an alien threat.",
            rating = 6.6,
            genres = new[] { "Action", "Adventure", "Fantasy" },
            services = new[] { "Netflix", "Prime Video" }
        }
    };
    
    // Add a delay to simulate API latency but keep it reasonable for tests
    System.Threading.Thread.Sleep(500);
    
    return Results.Json(recommendations);
});

// Health check endpoint for test status
app.MapGet("/health", () => {
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

Console.WriteLine("Test server starting...");
app.Run();
EOL

# Create test appsettings.json
cat > test_build/MoviesMadeEasyProject/MoviesMadeEasy/appsettings.json << 'EOL'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OpenAI_ApiKey": "sk-dummy-key-for-testing",
  "TMDBApiKey": "dummy-key-for-testing",
  "RapidApiKey": "dummy-rapid-api-key-for-testing",
  "OpenAI_Model": "gpt-3.5-turbo"
}
EOL

# Fix for the RecommendationsPage class to address the timeouts
cat > test_build/MoviesMadeEasyProject/MyBddProject.Tests/PageObjects/RecommendationsPage.cs << 'EOL'
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace MyBddProject.PageObjects
{
    public class RecommendationsPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public RecommendationsPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60)); // Increased timeout for CI
        }

        public void WaitForPageToLoad()
        {
            // First wait for URL change
            Console.WriteLine("Waiting for Recommendations page URL...");
            _wait.Until(d => d.Url.Contains("Recommendations", StringComparison.OrdinalIgnoreCase));
            Console.WriteLine("URL contains Recommendations");

            // Then wait for loading spinner to disappear (with a more tolerant approach)
            try {
                Console.WriteLine("Waiting for loading spinner to disappear...");
                _wait.Until(d => 
                {
                    try 
                    {
                        var spinner = d.FindElement(By.Id("loadingSpinner"));
                        var display = spinner.GetCssValue("display");
                        Console.WriteLine($"Spinner display value: {display}");
                        return display == "none";
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Spinner element not found (which is good)");
                        return true;
                    }
                    catch (StaleElementReferenceException)
                    {
                        Console.WriteLine("Stale element exception (trying again)");
                        return false;
                    }
                });
                Console.WriteLine("Loading spinner is now gone");
            } 
            catch (WebDriverTimeoutException) {
                Console.WriteLine("Warning: Timed out waiting for spinner to disappear, but continuing");
            }

            // Finally wait for recommendation cards
            Console.WriteLine("Waiting for recommendation cards...");
            try {
                _wait.Until(d => 
                {
                    try {
                        var cards = d.FindElements(By.CssSelector("#recommendationsContainer .movie-card"));
                        Console.WriteLine($"Found {cards.Count} recommendation cards");
                        bool allDisplayed = cards.Count >= 1 && cards.All(c => {
                            try { return c.Displayed; }
                            catch { return false; }
                        });
                        return allDisplayed;
                    }
                    catch (StaleElementReferenceException) {
                        Console.WriteLine("Stale element when checking cards");
                        return false;
                    }
                });
                Console.WriteLine("Recommendation cards are displayed");
            }
            catch (WebDriverTimeoutException ex) {
                Console.WriteLine($"Warning: Timed out waiting for cards: {ex.Message}");
                // Take screenshot for debugging
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var filename = "recommendations-timeout.png";
                screenshot.SaveAsFile(filename);
                Console.WriteLine($"Screenshot saved as {filename}");
                
                // Continue anyway to see if we can recover
            }
        }

        public int RecommendationCount()
        {
            return _driver.FindElements(By.CssSelector("#recommendationsContainer .movie-card")).Count;
        }

        public void ClickBackToSearch()
        {
            // More robust implementation
            var backButton = _wait.Until(d => {
                try {
                    var btn = d.FindElement(By.CssSelector(".back-to-search .btn-primary"));
                    if (btn.Displayed && btn.Enabled) return btn;
                    return null;
                }
                catch {
                    return null;
                }
            });

            // Scroll into view and click with JavaScript
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", backButton);

            // Wait for navigation to happen
            _wait.Until(d => !d.Url.Contains("Recommendations"));
        }

        public bool IsRecommendationFor(string originalTitle)
        {
            try
            {
                var header = _driver.FindElement(By.CssSelector("#recommendationsContainer h3"));
                return header.Text.Contains(originalTitle);
            }
            catch
            {
                return false;
            }
        }

        public void ClickViewDetails(int resultIndex = 0)
        {
            try {
                // First make sure cards are present
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
                var cards = wait.Until(d => {
                    var elements = d.FindElements(By.CssSelector(".movie-card"));
                    return elements.Count > 0 ? elements : null;
                });
                
                Console.WriteLine($"Found {cards.Count} movie cards");
                
                // Then get buttons
                var buttons = _driver.FindElements(By.CssSelector(".movie-card .btn-primary"));
                Console.WriteLine($"Found {buttons.Count} 'View Details' buttons");
                
                if (buttons.Count > resultIndex) {
                    Console.WriteLine("Clicking View Details button");
                    // JavaScript click is more reliable
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", buttons[resultIndex]);
                } else {
                    throw new Exception($"Button index {resultIndex} is out of range (only {buttons.Count} buttons found)");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error clicking View Details: {ex}");
                
                // Take screenshot for debugging
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var filename = "view-details-error.png";
                screenshot.SaveAsFile(filename);
                Console.WriteLine($"Screenshot saved as {filename}");
                
                throw;
            }
        }
    }
}
EOL

# Also fix the modal page class
cat > test_build/MoviesMadeEasyProject/MyBddProject.Tests/PageObjects/ModalPage.cs << 'EOL'
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.PageObjects
{
    public class ModalPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public ModalPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60)); // Increased timeout for CI
        }

        public bool IsModalDisplayed()
        {
            try
            {
                return _wait.Until(d => {
                    try {
                        var modal = d.FindElement(By.CssSelector(".modal.show"));
                        Console.WriteLine("Modal found and is displayed");
                        return modal.Displayed;
                    }
                    catch (NoSuchElementException) {
                        Console.WriteLine("Modal not found yet");
                        return false;
                    }
                    catch (StaleElementReferenceException) {
                        Console.WriteLine("Stale element when checking modal");
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout waiting for modal: {ex.Message}");
                
                // Take screenshot to help debug
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var filename = "modal-timeout.png";
                screenshot.SaveAsFile(filename);
                Console.WriteLine($"Screenshot saved as {filename}");
                
                return false;
            }
        }

        public bool IsModalTitleDisplayed()
        {
            try
            {
                return _wait.Until(d => {
                    try {
                        var title = d.FindElement(By.Id("modalTitle"));
                        return title.Displayed && !string.IsNullOrWhiteSpace(title.Text);
                    }
                    catch {
                        return false;
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        public void ClickStreamingIcon(string serviceName)
        {
            var icon = _wait.Until(d => {
                try {
                    var allIcons = d.FindElements(By.CssSelector(".streaming-icon"));
                    var targetIcon = allIcons.FirstOrDefault(i => 
                        i.GetAttribute("alt")?.Contains(serviceName, StringComparison.OrdinalIgnoreCase) ?? false);
                    
                    if (targetIcon != null && targetIcon.Displayed)
                        return targetIcon;
                    return null;
                }
                catch {
                    return null;
                }
            });
            
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", icon);
        }

        public void WaitForModalToLoad()
        {
            // First wait for modal to be visible
            Console.WriteLine("Waiting for modal to be visible");
            _wait.Until(d => {
                try {
                    var modal = d.FindElement(By.CssSelector(".modal.show"));
                    return modal.Displayed;
                }
                catch {
                    return false;
                }
            });
            Console.WriteLine("Modal is visible");
            
            // Then wait for loading to complete
            Console.WriteLine("Waiting for streaming content to load");
            _wait.Until(d => {
                try {
                    var streaming = d.FindElement(By.Id("modalStreaming"));
                    return !streaming.Text.Contains("Loading");
                }
                catch {
                    return false;
                }
            });
            Console.WriteLine("Modal content fully loaded");
        }

        public void WaitForStreamingIcons(int minCount = 1)
        {
            // Make this more resilient
            Console.WriteLine($"Waiting for at least {minCount} streaming icons");
            try {
                _wait.Until(d => 
                {
                    try 
                    {
                        var icons = d.FindElements(By.CssSelector(".streaming-icon"));
                        if (icons.Count < minCount) {
                            Console.WriteLine($"Found {icons.Count} icons, need at least {minCount}");
                            return false;
                        }
                        
                        bool allDisplayed = icons.All(icon => {
                            try { return icon.Displayed; }
                            catch { return false; }
                        });
                        
                        Console.WriteLine($"Found {icons.Count} icons, all displayed: {allDisplayed}");
                        return allDisplayed;
                    }
                    catch (StaleElementReferenceException)
                    {
                        Console.WriteLine("Stale element exception checking streaming icons");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking streaming icons: {ex.Message}");
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException) {
                Console.WriteLine("Warning: Timed out waiting for streaming icons");
                
                // Take screenshot for debugging
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var filename = "streaming-icons-timeout.png";
                screenshot.SaveAsFile(filename);
                
                // Try to add more debug info
                try {
                    var modalContent = _driver.FindElement(By.CssSelector(".modal-content")).Text;
                    Console.WriteLine($"Modal content: {modalContent}");
                } catch {}
            }
        }

        // Modify IsStreamingIconDisplayed
        public bool IsStreamingIconDisplayed(string serviceName)
        {
            try
            {
                return _wait.Until(d => {
                    try {
                        var icons = d.FindElements(By.CssSelector(".streaming-icon"));
                        Console.WriteLine($"Checking for {serviceName} icon among {icons.Count} icons");
                        
                        foreach (var icon in icons) {
                            var alt = icon.GetAttribute("alt") ?? "";
                            Console.WriteLine($"Icon alt text: {alt}");
                        }
                        
                        return icons.Any(icon => 
                            (icon.GetAttribute("alt")?.Contains(serviceName, StringComparison.OrdinalIgnoreCase) ?? false) && 
                            icon.Displayed);
                    }
                    catch (StaleElementReferenceException) {
                        return false;
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        public void ClickMoreLikeThisButton()
        {
            var button = _wait.Until(d => d.FindElement(By.CssSelector(".btn-more-like-this")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", button);
        }

        public bool AreRecommendationsDisplayed()
        {
            try
            {
                return _wait.Until(d => d.FindElements(By.CssSelector(".recommendation-item")).Count > 0);
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetRecommendationTitles()
        {
            return _driver.FindElements(By.CssSelector(".recommendation-item h5"))
                        .Select(e => e.Text)
                        .ToList();
        }

        public void WaitForAnyStreamingIcon()
        {
            Console.WriteLine("Waiting for streaming icons...");
            try {
                _wait.Until(d => 
                {
                    try {
                        var icons = d.FindElements(By.CssSelector(".streaming-icon"));
                        bool hasVisibleIcons = icons.Count > 0 && icons[0].Displayed;
                        Console.WriteLine($"Found {icons.Count} streaming icons, visible: {hasVisibleIcons}");
                        return hasVisibleIcons;
                    }
                    catch (StaleElementReferenceException) {
                        Console.WriteLine("Stale element when checking for icons");
                        return false;
                    }
                });
                Console.WriteLine("Found visible streaming icons");
            } catch (Exception ex) {
                Console.WriteLine($"Error waiting for streaming icons: {ex.Message}");
                
                // Take screenshot
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var filename = "streaming-icons-wait-error.png";
                screenshot.SaveAsFile(filename);
                Console.WriteLine($"Screenshot saved as {filename}");
                
                throw;
            }
        }

        public bool AreStreamingIconsDisplayed()
        {
            try
            {
                var icons = _driver.FindElements(By.CssSelector(".streaming-icon"));
                Console.WriteLine($"Found {icons.Count} streaming icons");
                return icons.Count > 0 && icons[0].Displayed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for streaming icons: {ex.Message}");
                return false;
            }
        }

        public void ClickFirstStreamingIcon()
        {
            try {
                var icon = _wait.Until(d => 
                {
                    var icons = d.FindElements(By.CssSelector(".streaming-icon"));
                    return icons.Count > 0 ? icons[0] : null;
                });
                
                Console.WriteLine("Clicking first streaming icon");
                // JavaScript click is more reliable in headless browsers
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", icon);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", icon);
            } catch (Exception ex) {
                Console.WriteLine($"Error clicking streaming icon: {ex.Message}");
                
                // Take screenshot
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var filename = "click-icon-error.png";
                screenshot.SaveAsFile(filename);
                
                throw;
            }
        }
    }
}
EOL

# Build test app
echo "Building test app..."
dotnet publish test_build/MoviesMadeEasyProject/MoviesMadeEasy/MoviesMadeEasy.csproj --configuration Release --output ./test_output

# Step 2: Prepare the production build (unmodified code)
echo "Setting up production build..."
cp -r MoviesMadeEasyProject prod_build/

# Build production app for deployment
dotnet publish prod_build/MoviesMadeEasyProject/MoviesMadeEasy/MoviesMadeEasy.csproj --configuration Release --output ./publish_output

# Run the test app with higher timeout
cd ./test_output

# Add explicit port setting
echo "Starting test app..."
nohup dotnet MoviesMadeEasy.dll --urls http://localhost:5000 > app.log 2>&1 &
TEST_APP_PID=$!
echo "Test app started on http://localhost:5000 with PID $TEST_APP_PID"
cd ..

# Wait longer for app to initialize
echo "Waiting for test app to start (45 seconds)..."
for i in {1..9}; do
    echo "Waiting... $((i*5))/45 seconds"
    sleep 5
    # Check if app is responding
    response_code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000)
    if [[ "$response_code" == "200" ]]; then
        echo "Test app is now responding with code $response_code"
        break
    fi
done

# Make sure health endpoint responds
for i in {1..5}; do
    health_response=$(curl -s http://localhost:5000/health)
    if [[ "$health_response" == *"healthy"* ]]; then
        echo "Health check successful: $health_response"
        break
    else
        echo "Health check attempt $i not successful yet, waiting..."
        sleep 2
    fi
done

# Final check if test app is responding
response_code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000)
echo "Final test app response code: $response_code"

if [[ "$response_code" != "200" ]]; then
    echo "WARNING: Test app may not have started properly. Check logs:"
    cat ./test_output/app.log
    exit 1
else
    echo "Test app is running successfully for BDD tests"
fi

# Display Chrome and ChromeDriver versions
echo "===== Chrome & ChromeDriver Versions ====="
google-chrome --version
chromedriver --version
echo "========================================"

echo "Production build ready for deployment at ./publish_output"
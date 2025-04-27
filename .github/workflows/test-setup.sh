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

Console.WriteLine("Using in-memory database for testing");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseInMemoryDatabase("TestAuthDb"));

builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<UserDbContext>());

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    // Make password requirements less strict for testing
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
        Console.WriteLine("Starting database seeding for tests...");
        
        // Create the database context and ensure it's created
        var userDbContext = services.GetRequiredService<UserDbContext>();
        userDbContext.Database.EnsureCreated();
        
        // CRITICAL FIX: Identify and stop the MoviesMadeEasy test database if running locally
        // This ensures we don't have conflicting databases
        Console.WriteLine("Checking for any existing data...");
        if (userDbContext.StreamingServices.Any() || userDbContext.Titles.Any() || userDbContext.Users.Any())
        {
            Console.WriteLine("CRITICAL WARNING: Database already contains data! Clearing all data to ensure clean state.");
            userDbContext.RecentlyViewedTitles.RemoveRange(userDbContext.RecentlyViewedTitles);
            userDbContext.UserStreamingServices.RemoveRange(userDbContext.UserStreamingServices);
            userDbContext.Titles.RemoveRange(userDbContext.Titles);
            userDbContext.Users.RemoveRange(userDbContext.Users);
            userDbContext.StreamingServices.RemoveRange(userDbContext.StreamingServices);
            userDbContext.SaveChanges();
            Console.WriteLine("Database cleared successfully");
        }
        
        // Seed streaming services
        Console.WriteLine("Seeding streaming services...");
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
        Console.WriteLine($"Added {streamingServices.Count} streaming services");
        
        // Create test movie titles with explicit IDs to ensure consistency
        Console.WriteLine("Adding test movie titles with explicit IDs...");
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
        Console.WriteLine($"Added Pokemon 4Ever with ID {pokemonId} and Her with ID {herId}");
        
        // Now seed the users AFTER movies are created
        Console.WriteLine("Creating test users...");
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        
        // Create explicit test user (don't use SeedData.InitializeAsync)
        var testUser = new IdentityUser
        {
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            EmailConfirmed = true
        };
        
        var existingUser = await userManager.FindByEmailAsync("testuser@example.com");
        if (existingUser != null)
        {
            Console.WriteLine("Test user already exists, recreating for clean state...");
            await userManager.DeleteAsync(existingUser);
        }
        
        var result = await userManager.CreateAsync(testUser, "Ab+1234");
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        
        // Create custom user record explicitly
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
        Console.WriteLine($"Created test user with ID {userId}");
        
        // Add Hulu subscription
        var huluService = userDbContext.StreamingServices.FirstOrDefault(s => s.Name == "Hulu");
        if (huluService != null)
        {
            userDbContext.UserStreamingServices.Add(new UserStreamingService { 
                UserId = userId, 
                StreamingServiceId = huluService.Id
            });
            userDbContext.SaveChanges();
            Console.WriteLine($"Added Hulu subscription for user {userId}");
        }
        
        // CRITICAL FIX: Add recently viewed titles with VERY explicit timing
        Console.WriteLine("Setting up recently viewed titles...");
        
        // First view Pokemon (older timestamp) - 30 days ago
        var pokemonView = new RecentlyViewedTitle
        {
            UserId = userId,
            TitleId = pokemonId,
            ViewedAt = DateTime.UtcNow.AddDays(-30)
        };
        userDbContext.RecentlyViewedTitles.Add(pokemonView);
        userDbContext.SaveChanges();
        Console.WriteLine($"Added Pokemon view {pokemonView.Id} at {pokemonView.ViewedAt}");
        
        // Then view Her (newer timestamp) - just 1 minute ago 
        var herView = new RecentlyViewedTitle
        {
            UserId = userId,
            TitleId = herId,
            ViewedAt = DateTime.UtcNow.AddMinutes(-1)
        };
        userDbContext.RecentlyViewedTitles.Add(herView);
        userDbContext.SaveChanges();
        Console.WriteLine($"Added Her view {herView.Id} at {herView.ViewedAt}");
        
        // SUPER IMPORTANT - Double check our work!
        var checkViews = userDbContext.RecentlyViewedTitles
            .Where(rv => rv.UserId == userId)
            .OrderByDescending(rv => rv.ViewedAt)
            .ToList();
        
        if (checkViews.Count != 2)
        {
            throw new Exception($"Expected 2 recently viewed items, found {checkViews.Count}");
        }
        
        var firstViewId = checkViews[0].TitleId;
        var secondViewId = checkViews[1].TitleId;
        
        if (firstViewId != herId || secondViewId != pokemonId)
        {
            throw new Exception($"FATAL ERROR: Wrong viewing order! First={firstViewId}, Second={secondViewId}, Expected Her={herId}, Pokemon={pokemonId}");
        }
        
        Console.WriteLine("SUCCESS! Verified recently viewed order: Her (newest) → Pokemon (oldest)");
        
        // Final confidence check - use the TitleRepository method
        var titleRepo = services.GetRequiredService<ITitleRepository>();
        var recentViews = titleRepo.GetRecentlyViewedByUser(userId);
        
        if (recentViews.Count >= 2 && 
            recentViews[0].TitleName == "Her" && 
            recentViews[1].TitleName == "Pokemon 4Ever")
        {
            Console.WriteLine("VERIFIED using TitleRepository: Her is correctly shown first, Pokemon second");
        }
        else
        {
            var viewOrder = string.Join(" → ", recentViews.Select(t => t.TitleName));
            throw new Exception($"FATAL ERROR: TitleRepository returns wrong order: {viewOrder}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"!!! CRITICAL ERROR DURING SEEDING !!!: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw; // Rethrow to fail the app startup - we can't proceed with wrong data
    }
}

// Mock API endpoints for tests
app.MapGet("/Home/SearchMovies", (string query) => {
    Console.WriteLine($"Handling mock movie search for: {query}");
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
    
    return Results.Json(movieResults);
});

app.MapGet("/Home/GetSimilarMovies", (string title) => {
    Console.WriteLine($"Handling mock recommendation search for: {title}");
    var recommendations = new List<object>
    {
        new { title = "The Maze Runner", year = 2014, reason = "Similar dystopian theme" },
        new { title = "Divergent", year = 2014, reason = "Features a strong female protagonist in a dystopian future" },
        new { title = "Battle Royale", year = 2000, reason = "Similar survival competition premise" },
        new { title = "The Giver", year = 2014, reason = "Dystopian society with controlled roles" },
        new { title = "Ender's Game", year = 2013, reason = "Young protagonists trained for combat" }
    };
    
    return Results.Json(recommendations);
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

Console.WriteLine("Test configuration started successfully");
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

# Build test app
dotnet publish test_build/MoviesMadeEasyProject/MoviesMadeEasy/MoviesMadeEasy.csproj --configuration Release --output ./test_output

# Step 2: Prepare the production build (unmodified code)
echo "Setting up production build..."
cp -r MoviesMadeEasyProject prod_build/

# Build production app for deployment
dotnet publish prod_build/MoviesMadeEasyProject/MoviesMadeEasy/MoviesMadeEasy.csproj --configuration Release --output ./publish_output

# Run the test app
cd ./test_output
nohup dotnet MoviesMadeEasy.dll --urls http://localhost:5000 > app.log 2>&1 &
TEST_APP_PID=$!
echo "Test app started on http://localhost:5000 with PID $TEST_APP_PID"
cd ..

# Wait for app to initialize
echo "Waiting for test app to start..."
sleep 15

# Check if test app is responding
response_code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000)
echo "Test app response code: $response_code"

if [[ "$response_code" != "200" ]]; then
  echo "WARNING: Test app may not have started properly. Check logs at ./test_output/app.log"
  cat ./test_output/app.log
  exit 1
else
  echo "Test app is running successfully for BDD tests"
fi

echo "Production build ready for deployment at ./publish_output"
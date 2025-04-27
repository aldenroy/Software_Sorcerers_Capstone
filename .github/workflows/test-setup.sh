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
        
        // Seed streaming services
        if (!userDbContext.StreamingServices.Any())
        {
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
        }
        
        // Create test movie titles FIRST (before seeding users)
        if (!userDbContext.Titles.Any(t => t.TitleName == "Pokemon 4Ever" || t.TitleName == "Her"))
        {
            Console.WriteLine("Adding test movie titles...");
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
            
            userDbContext.Titles.Add(herMovie);
            userDbContext.Titles.Add(pokemonMovie);
            userDbContext.SaveChanges();
            Console.WriteLine("Test movie titles added successfully");
        }
        
        // Seed users
        await SeedData.InitializeAsync(services);
        Console.WriteLine("User seeding completed");
        
        // Setup user subscriptions and recently viewed titles AFTER users are created
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var testUser1 = await userManager.FindByEmailAsync("testuser@example.com");
        
        if (testUser1 != null)
        {
            Console.WriteLine($"Found test user: {testUser1.Email}");
            var userRepo = services.GetRequiredService<IUserRepository>();
            var user = userRepo.GetUser(testUser1.Id);
            
            if (user != null)
            {
                Console.WriteLine($"Found corresponding user record with ID: {user.Id}");
                
                // Add Hulu subscription
                var huluService = userDbContext.StreamingServices.FirstOrDefault(s => s.Name == "Hulu");
                if (huluService != null && !userDbContext.UserStreamingServices.Any(us => us.UserId == user.Id && us.StreamingServiceId == huluService.Id))
                {
                    userDbContext.UserStreamingServices.Add(new UserStreamingService { 
                        UserId = user.Id, 
                        StreamingServiceId = huluService.Id
                    });
                    userDbContext.SaveChanges();
                    Console.WriteLine("Added Hulu subscription to test user");
                }
                
                // Setup recently viewed titles with CORRECT ordering
                var herTitle = userDbContext.Titles.FirstOrDefault(t => t.TitleName == "Her");
                var pokemonTitle = userDbContext.Titles.FirstOrDefault(t => t.TitleName == "Pokemon 4Ever");
                
                if (herTitle != null && pokemonTitle != null)
                {
                    Console.WriteLine("Setting up recently viewed titles with Her being more recent than Pokemon 4Ever");
                    
                    // Remove any existing views first to prevent ordering issues
                    var existingViews = userDbContext.RecentlyViewedTitles
                        .Where(rv => rv.UserId == user.Id && 
                               (rv.TitleId == herTitle.Id || rv.TitleId == pokemonTitle.Id))
                        .ToList();
                        
                    if (existingViews.Any())
                    {
                        Console.WriteLine($"Removing {existingViews.Count} existing recently viewed entries");
                        userDbContext.RecentlyViewedTitles.RemoveRange(existingViews);
                        userDbContext.SaveChanges();
                    }
                    
                    // First add Pokemon 4Ever (older timestamp)
                    Console.WriteLine("Adding Pokemon 4Ever with older timestamp");
                    userDbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
                    {
                        UserId = user.Id,
                        TitleId = pokemonTitle.Id,
                        ViewedAt = DateTime.UtcNow.AddHours(-2) // Older timestamp
                    });
                    userDbContext.SaveChanges();
                    
                    // Then add Her with a newer timestamp
                    Console.WriteLine("Adding Her with newer timestamp");
                    userDbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
                    {
                        UserId = user.Id,
                        TitleId = herTitle.Id,
                        ViewedAt = DateTime.UtcNow // More recent timestamp
                    });
                    userDbContext.SaveChanges();
                    
                    Console.WriteLine("Recently viewed titles created with Her having more recent timestamp than Pokemon 4Ever");
                    
                    // Verify the order just to be sure
                    var checkOrder = userDbContext.RecentlyViewedTitles
                        .Where(rv => rv.UserId == user.Id)
                        .OrderByDescending(rv => rv.ViewedAt)
                        .ToList();
                    
                    if (checkOrder.Count >= 2)
                    {
                        var first = userDbContext.Titles.Find(checkOrder[0].TitleId);
                        var second = userDbContext.Titles.Find(checkOrder[1].TitleId);
                        Console.WriteLine($"Verified order: 1st={first?.TitleName} (at {checkOrder[0].ViewedAt}), 2nd={second?.TitleName} (at {checkOrder[1].ViewedAt})");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: Could not find one or both movie titles!");
                }
            }
            else
            {
                Console.WriteLine("ERROR: Could not find user record for test user!");
            }
        }
        else
        {
            Console.WriteLine("ERROR: Could not find test user in identity system!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred seeding the DB: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
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
echo "Test app started on http://localhost:5000"
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
else
  echo "Test app is running successfully for BDD tests"
fi

echo "Production build ready for deployment at ./publish_output"
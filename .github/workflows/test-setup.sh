#!/bin/bash

# Create temporary directory for CI build
mkdir -p ci_build

# Copy entire project excluding Program.cs
cp -r MoviesMadeEasyProject ci_build/

# Create modified Program.cs for CI
cat > ci_build/MoviesMadeEasyProject/MoviesMadeEasy/Program.cs << 'EOL'
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.DAL.Concrete;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Microsoft.AspNetCore.Session;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// For CI/CD environment, add API keys directly
builder.Configuration["OpenAI_ApiKey"] = "sk-dummy-key-for-testing";
builder.Configuration["TMDBApiKey"] = "dummy-key-for-testing";
builder.Configuration["RapidApiKey"] = "dummy-rapid-api-key-for-testing";
builder.Configuration["OpenAI_Model"] = "gpt-3.5-turbo";

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
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

Console.WriteLine("Using in-memory database for CI/CD");
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

// THIS IS THE CRITICAL PART - Seed the test users and streaming services
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("Starting database seeding...");
        
        // Create the database context and ensure it's created
        var userDbContext = services.GetRequiredService<UserDbContext>();
        userDbContext.Database.EnsureCreated();
        
        // Seed streaming services first
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
        else
        {
            Console.WriteLine($"Streaming services already exist: {userDbContext.StreamingServices.Count()}");
        }
        
        // Now seed the users with SeedData.InitializeAsync
        await SeedData.InitializeAsync(services);
        Console.WriteLine("User seeding completed successfully");
        
        // Create test movie titles if they don't exist
        if (!userDbContext.Titles.Any(t => t.TitleName == "Pokemon 4Ever" || t.TitleName == "Her"))
        {
            Console.WriteLine("Adding test movie titles for recently viewed tests...");
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
            
            userDbContext.Titles.Add(pokemonMovie);
            userDbContext.Titles.Add(herMovie);
            userDbContext.SaveChanges();
            
            Console.WriteLine("Test movie titles added successfully");
        }
        
        // Verify users and services for debugging
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var testUser1 = userManager.FindByEmailAsync("testuser@example.com").Result;
        var testUser2 = userManager.FindByEmailAsync("testuser2@example.com").Result;
        
        Console.WriteLine($"Test User 1: {(testUser1 != null ? "Created" : "Missing")}");
        Console.WriteLine($"Test User 2: {(testUser2 != null ? "Created" : "Missing")}");
        
        // Verify subscriptions
        var userRepo = services.GetRequiredService<IUserRepository>();
        var subRepo = services.GetRequiredService<ISubscriptionRepository>();
        
        if (testUser1 != null)
        {
            var user = userRepo.GetUser(testUser1.Id);
            if (user != null)
            {
                var subs = subRepo.GetUserSubscriptions(user.Id);
                Console.WriteLine($"User has {subs?.Count() ?? 0} subscriptions");
                
                // Make sure user has Hulu subscription
                var huluService = userDbContext.StreamingServices.FirstOrDefault(s => s.Name == "Hulu");
                if (huluService != null && !userDbContext.UserStreamingServices.Any(us => us.UserId == user.Id && us.StreamingServiceId == huluService.Id))
                {
                    Console.WriteLine("Adding Hulu subscription to testuser");
                    userDbContext.UserStreamingServices.Add(new UserStreamingService { 
                        UserId = user.Id, 
                        StreamingServiceId = huluService.Id
                    });
                    userDbContext.SaveChanges();
                }
                
                // Ensure proper order of recently viewed titles for tests
                var herTitle = userDbContext.Titles.FirstOrDefault(t => t.TitleName == "Her");
                var pokemonTitle = userDbContext.Titles.FirstOrDefault(t => t.TitleName == "Pokemon 4Ever");
                
                if (herTitle != null && pokemonTitle != null)
                {
                    // Update the timestamps to ensure Her is more recent
                    var herView = userDbContext.RecentlyViewedTitles.FirstOrDefault(r => r.UserId == user.Id && r.TitleId == herTitle.Id);
                    var pokemonView = userDbContext.RecentlyViewedTitles.FirstOrDefault(r => r.UserId == user.Id && r.TitleId == pokemonTitle.Id);
                    
                    // If views exist, update them, otherwise create new ones
                    if (herView == null)
                    {
                        herView = new RecentlyViewedTitle
                        {
                            UserId = user.Id,
                            TitleId = herTitle.Id,
                            ViewedAt = DateTime.UtcNow // More recent
                        };
                        userDbContext.RecentlyViewedTitles.Add(herView);
                    }
                    else
                    {
                        herView.ViewedAt = DateTime.UtcNow;
                    }
                    
                    if (pokemonView == null)
                    {
                        pokemonView = new RecentlyViewedTitle
                        {
                            UserId = user.Id,
                            TitleId = pokemonTitle.Id,
                            ViewedAt = DateTime.UtcNow.AddMinutes(-30) // Older
                        };
                        userDbContext.RecentlyViewedTitles.Add(pokemonView);
                    }
                    else
                    {
                        pokemonView.ViewedAt = DateTime.UtcNow.AddMinutes(-30);
                    }
                    
                    userDbContext.SaveChanges();
                    Console.WriteLine("Updated view times to ensure Her is more recent than Pokemon 4Ever");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred seeding the DB: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

// Mock API endpoints for BDD tests
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

// Mock API response for recommendations
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

app.Run();
EOL

# Create CI appsettings.json
cat > ci_build/MoviesMadeEasyProject/MoviesMadeEasy/appsettings.json << 'EOL'
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

# Build from the CI directory
dotnet publish ci_build/MoviesMadeEasyProject/MoviesMadeEasy/MoviesMadeEasy.csproj --configuration Release --output ./publish_output

# Run the app
cd ./publish_output
nohup dotnet MoviesMadeEasy.dll --urls http://localhost:5000 > app.log 2>&1 &
cd ..
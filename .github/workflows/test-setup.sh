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

// Mock API endpoints for tests
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
    
    return Results.Json(movieResults);
});

app.MapGet("/Home/GetSimilarMovies", (string title) => {
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
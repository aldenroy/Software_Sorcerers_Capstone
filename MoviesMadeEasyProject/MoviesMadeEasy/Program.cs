using MoviesMadeEasy.DAL.Abstract; // Add this line
using MoviesMadeEasy.DAL.Concrete; // Add this line
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
}

// Register HttpClient for MovieService
builder.Services.AddHttpClient<IMovieService, MovieService>();
builder.Services.AddScoped<IMovieService, MovieService>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new MovieService(httpClient, configuration);
});

builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var azurePublish = false;

var connectionString = builder.Configuration.GetConnectionString(
    azurePublish ? "AzureConnection" : "DefaultConnection") ??
    throw new InvalidOperationException("Connection string not found.");

var authConnectionString = builder.Configuration.GetConnectionString(
    azurePublish ? "AzureIdentityConnection" : "IdentityConnection") ??
    throw new InvalidOperationException("Connection string not found.");


builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseLazyLoadingProxies().UseSqlServer(connectionString));

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseLazyLoadingProxies().UseSqlServer(authConnectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<IdentityDbContext>();


builder.Services.AddRazorPages();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); 

app.Run();
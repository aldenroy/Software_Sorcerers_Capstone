using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>(); 

        string testEmail = "testuser@example.com";
        string password = "Ab+1234";

        var user = await userManager.FindByEmailAsync(testEmail);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = testEmail,
                Email = testEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create test user: " + string.Join(", ", result.Errors));
            }

            MoviesMadeEasy.Models.User customUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.AspNetUserId == user.Id);

            if (customUser == null)
            {
                customUser = new MoviesMadeEasy.Models.User
                {
                    AspNetUserId = user.Id,
                    FirstName = "Test",
                    LastName = "User",
                    ColorMode = "Light",
                    FontSize = "Medium",
                    FontType = "Sans-serif"
                };

                dbContext.Users.Add(customUser);
                await dbContext.SaveChangesAsync(); 
            }

            var existingServiceIds = dbContext.UserStreamingServices
                .Where(uss => uss.UserId == customUser.Id)
                .Select(uss => uss.StreamingServiceId)
                .ToHashSet();

            var serviceNames = new[] { "Hulu", "Disney+", "Netflix" };
            var matchingServices = await dbContext.StreamingServices
                .Where(s => serviceNames.Contains(s.Name))
                .ToListAsync();

            foreach (var service in matchingServices)
            {
                dbContext.UserStreamingServices.Add(new UserStreamingService
                {
                    UserId = customUser.Id,
                    StreamingServiceId = service.Id
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}


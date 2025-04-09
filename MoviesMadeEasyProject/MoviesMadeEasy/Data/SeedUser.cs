using Microsoft.AspNetCore.Identity;
using MoviesMadeEasy.Data;

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

            dbContext.Users.Add(new MoviesMadeEasy.Models.User
            {
                AspNetUserId = user.Id,
                FirstName = "Test",
                LastName = "User",
                ColorMode = "Light",
                FontSize = "Medium",
                FontType = "Sans-serif"
            });

            await dbContext.SaveChangesAsync();
        }
    }
}


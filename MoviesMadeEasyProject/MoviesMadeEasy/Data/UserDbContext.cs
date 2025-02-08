using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.Data
{
    public class UserDbContext : IdentityDbContext<User>
    {
        public UserDbContext (DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the relationship between User and StreamingService
            builder.Entity<User>()
                .HasOne(u => u.StreamingServices)
                .WithMany()  // No need to include Users in StreamingService
                .HasForeignKey(u => u.StreamingServicesId)
                .OnDelete(DeleteBehavior.SetNull); // Or any behavior you prefer
        }

    }
}

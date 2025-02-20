using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;

namespace MoviesMadeEasy.Data
{
    public class UserDbContext : IdentityDbContext<User>
    {
        public UserDbContext (DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<StreamingService> StreamingServices { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<UserStreamingService> UserStreamingServices { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define Many-to-Many Relationship via Join Table
            builder.Entity<UserStreamingService>()
                .HasKey(us => new { us.UserId, us.StreamingServiceId }); // Composite key

            builder.Entity<UserStreamingService>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserStreamingServices) 
                .HasForeignKey(us => us.UserId);

            builder.Entity<UserStreamingService>()
                .HasOne(us => us.StreamingService)
                .WithMany(s => s.UserStreamingServices) 
                .HasForeignKey(us => us.StreamingServiceId);

            // Call the separate seeding class
            StreamingSeedData.SeedStreamingServices(builder);


            builder.Entity<StreamingService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("StreamingService");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
                entity.Property(e => e.Region)
                    .HasMaxLength(50)
                    .HasColumnName("region");
                entity.Property(e => e.BaseUrl)
                    .HasColumnType("nvarchar(max)")
                    .HasColumnName("base_url");
                entity.Property(e => e.LogoUrl)
                    .HasColumnType("nvarchar(max)")
                    .HasColumnName("logo_url");
            });

            builder.Entity<Title>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Title");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.ExternalId)
                    .HasMaxLength(255)
                    .HasColumnName("external_id");
                entity.Property(e => e.LastUpdated)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("last_updated");
                entity.Property(e => e.TitleName)
                    .HasMaxLength(255)
                    .HasColumnName("title_name");
                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .HasColumnName("type");
                entity.Property(e => e.Year).HasColumnName("year");
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<StreamingService> StreamingServices { get; set; }
        public virtual DbSet<Title> Titles { get; set; }
        public virtual DbSet<UserStreamingService> UserStreamingServices { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure join table for UserStreamingService
            builder.Entity<UserStreamingService>()
                .HasKey(us => new { us.UserId, us.StreamingServiceId });

            builder.Entity<UserStreamingService>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserStreamingServices)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserStreamingService>()
                .HasOne(us => us.StreamingService)
                .WithMany(s => s.UserStreamingServices)
                .HasForeignKey(us => us.StreamingServiceId); // Ensure this is INT

            // Configure StreamingService table
            builder.Entity<StreamingService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("StreamingService");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd() // Allow EF to handle IDENTITY columns
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

            // Configure Title table
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

                entity.Property(e => e.Year)
                    .HasColumnName("year");
            });

            // Configure custom User table
            builder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                      .ValueGeneratedOnAdd()
                      .HasColumnName("Id");

                // Map AspNetUsersId to the column "AspNetUserId" from your up script
                entity.Property(u => u.AspNetUserId)
                      .IsRequired()
                      .HasColumnName("AspNetUserId");

                entity.Property(u => u.FirstName)
                      .IsRequired()
                      .HasColumnName("FirstName");

                entity.Property(u => u.LastName)
                      .IsRequired()
                      .HasColumnName("LastName");

                entity.Property(u => u.RecentlyViewedShowId)
                      .HasColumnName("RecentlyViewedShowId");

                entity.Property(u => u.ColorMode)
                      .IsRequired()
                      .HasDefaultValue("")
                      .HasColumnName("ColorMode");

                entity.Property(u => u.FontSize)
                      .IsRequired()
                      .HasDefaultValue("")
                      .HasColumnName("FontSize");

                entity.Property(u => u.FontType)
                      .IsRequired()
                      .HasDefaultValue("")
                      .HasColumnName("FontType");

                // Explicitly configure the one-to-many relationship with Title:
                entity.HasOne(u => u.RecentlyViewedShow)
                    .WithMany(t => t.Users)
                    .HasForeignKey(u => u.RecentlyViewedShowId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

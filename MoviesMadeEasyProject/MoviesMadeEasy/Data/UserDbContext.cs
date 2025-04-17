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
        public virtual DbSet<RecentlyViewedTitle> RecentlyViewedTitles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
                .HasForeignKey(us => us.StreamingServiceId);

            builder.Entity<StreamingService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("StreamingService");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
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

                entity.Property(e => e.TitleName)
                    .HasMaxLength(255)
                    .HasColumnName("title_name");

                entity.Property(e => e.Year)
                    .HasColumnName("year");

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .HasColumnName("type");

                entity.Property(e => e.PosterUrl)
                    .HasColumnType("nvarchar(max)")
                    .HasColumnName("poster_url");

                entity.Property(e => e.Genres)
                    .HasColumnType("nvarchar(max)")
                    .HasColumnName("genres");

                entity.Property(e => e.Rating)
                    .HasMaxLength(50)
                    .HasColumnName("rating");

                entity.Property(e => e.Overview)
                    .HasColumnType("nvarchar(max)")
                    .HasColumnName("overview");

                entity.Property(e => e.StreamingServices)
                    .HasColumnType("nvarchar(max)")
                    .HasColumnName("streaming_services");

                entity.Property(e => e.LastUpdated)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("last_updated");

                entity.HasMany(t => t.RecentlyViewedTitles)
                      .WithOne(rv => rv.Title)
                      .HasForeignKey(rv => rv.TitleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<RecentlyViewedTitle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("RecentlyViewedTitles");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("Id");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserId");

                entity.Property(e => e.TitleId)
                    .HasColumnName("TitleId");

                entity.Property(e => e.ViewedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("ViewedAt");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RecentlyViewedTitles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Title)
                    .WithMany(t => t.RecentlyViewedTitles)
                    .HasForeignKey(e => e.TitleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.TitleId })
                    .IsUnique();
            });

            builder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                      .ValueGeneratedOnAdd()
                      .HasColumnName("Id");

                entity.Property(u => u.AspNetUserId)
                      .IsRequired()
                      .HasColumnName("AspNetUserId");

                entity.Property(u => u.FirstName)
                      .IsRequired()
                      .HasColumnName("FirstName");

                entity.Property(u => u.LastName)
                      .IsRequired()
                      .HasColumnName("LastName");

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

                entity.HasMany(u => u.RecentlyViewedTitles)
                      .WithOne(rv => rv.User)
                      .HasForeignKey(rv => rv.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

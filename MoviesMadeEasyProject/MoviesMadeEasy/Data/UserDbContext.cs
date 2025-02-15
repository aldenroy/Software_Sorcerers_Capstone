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

        public DbSet<StreamingService> StreamingServices { get; set; }
        public DbSet<Title> Titles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the relationship between User and StreamingService
            builder.Entity<User>()
                .HasOne(u => u.StreamingServices)
                .WithMany()  
                .HasForeignKey(u => u.StreamingServicesId)
                .OnDelete(DeleteBehavior.SetNull); 

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

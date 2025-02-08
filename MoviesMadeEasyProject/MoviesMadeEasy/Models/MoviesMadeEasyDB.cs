using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MoviesMadeEasy.Models;

public partial class MoviesMadeEasyDB : DbContext
{
    public MoviesMadeEasyDB()
    {
    }

    public MoviesMadeEasyDB(DbContextOptions<MoviesMadeEasyDB> options)
        : base(options)
    {
    }

    public virtual DbSet<StreamingService> StreamingServices { get; set; }

    public virtual DbSet<Title> Titles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MoviesMadeEasyDB;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StreamingService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Streamin__3213E83F560762FF");

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

        modelBuilder.Entity<Title>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Title__3213E83F9A992499");

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.AspnetIdentityId).HasName("PK__User__70553590AC122839");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__AB6E6164C9051DC9").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__User__F3DBC572F390B265").IsUnique();

            entity.Property(e => e.AspnetIdentityId)
                .ValueGeneratedNever()
                .HasColumnName("ASPNetIdentity_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(512)
                .HasColumnName("password_hash");
            entity.Property(e => e.RecentlyViewedShowId).HasColumnName("recently_viewed_show_id");
            entity.Property(e => e.StreamingServicesId).HasColumnName("streaming_services_id");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");

            entity.HasOne(d => d.RecentlyViewedShow).WithMany(p => p.Users)
                .HasForeignKey(d => d.RecentlyViewedShowId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_User_Title");

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

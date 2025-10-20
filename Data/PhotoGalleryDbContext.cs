using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Entities;

namespace PhotoGallery.Data;

public partial class PhotoGalleryDbContext : DbContext
{
    public PhotoGalleryDbContext(DbContextOptions<PhotoGalleryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Photo> Photos { get; set; }

    public virtual DbSet<PhotoTag> PhotoTags { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<ViPhoto> ViPhotos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Photos__3214EC0769CA2021");

            entity.Property(e => e.UploadedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<PhotoTag>(entity =>
        {
            entity.HasKey(e => new { e.PhotoId, e.TagId }).HasName("PK_PhotoTagMappings");

            entity.HasOne(d => d.Tag).WithMany(p => p.PhotoTags).HasConstraintName("FK_PhotoTagMappings_Tags");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC07A5844490");
        });

        modelBuilder.Entity<ViPhoto>(entity =>
        {
            entity.ToView("VI_Photos");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

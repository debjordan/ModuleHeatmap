using Microsoft.EntityFrameworkCore;
using ModuleHeatMap.Core.Entities;
using ModuleHeatMap.Core.Enums;
using System.Text.Json;

namespace ModuleHeatMap.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ModuleAccess> ModuleAccesses { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Application> Applications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ModuleAccess Configuration
        modelBuilder.Entity<ModuleAccess>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicationId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ModuleName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ModuleUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.AccessType)
                .HasConversion<int>();

            entity.Property(e => e.AccessedAt)
                .IsRequired();

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                );

            entity.HasIndex(e => new { e.ApplicationId, e.ModuleName });
            entity.HasIndex(e => new { e.ApplicationId, e.UserId });
            entity.HasIndex(e => e.AccessedAt);
        });

        // Module Configuration
        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicationId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Path)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasMany(e => e.Accesses)
                .WithOne()
                .HasForeignKey("ModuleId")
                .IsRequired(false);

            entity.HasIndex(e => new { e.ApplicationId, e.Name })
                .IsUnique();
        });

        // Application Configuration
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicationId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasMany(e => e.Modules)
                .WithOne()
                .HasForeignKey("ApplicationId")
                .HasPrincipalKey("ApplicationId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ApplicationId)
                .IsUnique();
        });
    }
}

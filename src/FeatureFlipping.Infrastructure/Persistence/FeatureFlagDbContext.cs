using FeatureFlipping.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlipping.Infrastructure.Persistence;

/// <summary>EF Core DbContext for feature flags.</summary>
public sealed class FeatureFlagDbContext : DbContext
{
    /// <summary>Initializes a new instance.</summary>
    public FeatureFlagDbContext(DbContextOptions<FeatureFlagDbContext> options) : base(options) { }

    /// <summary>Feature flags table.</summary>
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Key).IsRequired().HasMaxLength(200);
            entity.HasIndex(f => f.Key).IsUnique();
            entity.Property(f => f.Value).HasMaxLength(2000);
            entity.Property(f => f.UserTargeting).HasMaxLength(2000);
        });
    }
}

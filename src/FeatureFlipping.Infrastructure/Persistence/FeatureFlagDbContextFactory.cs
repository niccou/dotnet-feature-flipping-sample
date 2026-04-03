using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FeatureFlipping.Infrastructure.Persistence;

/// <summary>Design-time factory for EF Core migrations.</summary>
public sealed class FeatureFlagDbContextFactory : IDesignTimeDbContextFactory<FeatureFlagDbContext>
{
    /// <inheritdoc/>
    public FeatureFlagDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FeatureFlagDbContext>()
            .UseSqlite("Data Source=featureflags.db")
            .Options;
        return new FeatureFlagDbContext(options);
    }
}

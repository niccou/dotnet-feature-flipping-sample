using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlipping.Infrastructure.Persistence;

/// <summary>EF Core implementation of <see cref="IFeatureFlagRepository"/>.</summary>
public sealed class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly FeatureFlagDbContext _context;

    /// <summary>Initializes the repository.</summary>
    public FeatureFlagRepository(FeatureFlagDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeatureFlag>> GetAllAsync(CancellationToken ct = default) =>
        await _context.FeatureFlags.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<FeatureFlag?> GetByKeyAsync(FlagKey key, CancellationToken ct = default) =>
        await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Key == key.Value, ct);

    /// <inheritdoc/>
    public async Task AddAsync(FeatureFlag flag, CancellationToken ct = default)
    {
        _context.FeatureFlags.Add(flag);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(FeatureFlag flag, CancellationToken ct = default)
    {
        _context.FeatureFlags.Update(flag);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(FlagKey key, CancellationToken ct = default) =>
        await _context.FeatureFlags.AnyAsync(f => f.Key == key.Value, ct);
}

using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.ValueObjects;

namespace FeatureFlipping.Domain.Interfaces;

/// <summary>Repository abstraction for feature flags.</summary>
public interface IFeatureFlagRepository
{
    /// <summary>Gets all feature flags.</summary>
    Task<IReadOnlyList<FeatureFlag>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Gets a feature flag by its key.</summary>
    Task<FeatureFlag?> GetByKeyAsync(FlagKey key, CancellationToken ct = default);

    /// <summary>Adds a new feature flag.</summary>
    Task AddAsync(FeatureFlag flag, CancellationToken ct = default);

    /// <summary>Updates an existing feature flag.</summary>
    Task UpdateAsync(FeatureFlag flag, CancellationToken ct = default);

    /// <summary>Checks if a flag with the given key exists.</summary>
    Task<bool> ExistsAsync(FlagKey key, CancellationToken ct = default);
}

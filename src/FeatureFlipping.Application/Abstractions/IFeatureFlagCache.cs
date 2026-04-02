using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.ValueObjects;

namespace FeatureFlipping.Application.Abstractions;

/// <summary>Cache abstraction for feature flags (ISP).</summary>
public interface IFeatureFlagCache
{
    /// <summary>Tries to get a flag from the cache.</summary>
    Task<FeatureFlag?> GetAsync(FlagKey key);

    /// <summary>Stores a flag in the cache.</summary>
    Task SetAsync(FlagKey key, FeatureFlag flag);

    /// <summary>Invalidates a flag in the cache.</summary>
    Task InvalidateAsync(FlagKey key);

    /// <summary>Checks if a flag is currently cached.</summary>
    Task<bool> IsCachedAsync(FlagKey key);
}

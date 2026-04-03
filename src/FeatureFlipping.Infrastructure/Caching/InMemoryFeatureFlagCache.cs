using FeatureFlipping.Application.Abstractions;
using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;

namespace FeatureFlipping.Infrastructure.Caching;

/// <summary>In-memory cache implementation for feature flags with 30-second TTL.</summary>
public sealed class InMemoryFeatureFlagCache : IFeatureFlagCache
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    /// <summary>Initializes the cache.</summary>
    public InMemoryFeatureFlagCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    private static string CacheKey(FlagKey key) => $"ff:{key.Value}";
    private static string TrackerKey(FlagKey key) => $"ff-tracked:{key.Value}";

    /// <inheritdoc/>
    public Task<FeatureFlag?> GetAsync(FlagKey key)
    {
        _cache.TryGetValue(CacheKey(key), out FeatureFlag? flag);
        return Task.FromResult(flag);
    }

    /// <inheritdoc/>
    public Task SetAsync(FlagKey key, FeatureFlag flag)
    {
        _cache.Set(CacheKey(key), flag, Ttl);
        _cache.Set(TrackerKey(key), true, Ttl);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task InvalidateAsync(FlagKey key)
    {
        _cache.Remove(CacheKey(key));
        _cache.Remove(TrackerKey(key));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> IsCachedAsync(FlagKey key)
    {
        return Task.FromResult(_cache.TryGetValue(TrackerKey(key), out _));
    }
}

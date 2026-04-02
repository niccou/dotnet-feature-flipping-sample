using FeatureFlipping.Domain.ValueObjects;

namespace FeatureFlipping.Domain.Interfaces;

/// <summary>Evaluates feature flags for a given context.</summary>
public interface IFeatureFlagEvaluator
{
    /// <summary>Evaluates a feature flag for an optional user.</summary>
    Task<FlagEvaluationResult> EvaluateAsync(FlagKey key, string? userId = null, CancellationToken ct = default);
}

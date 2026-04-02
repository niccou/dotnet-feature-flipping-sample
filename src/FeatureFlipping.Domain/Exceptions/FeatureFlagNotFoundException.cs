namespace FeatureFlipping.Domain.Exceptions;

/// <summary>Thrown when a feature flag cannot be found.</summary>
public sealed class FeatureFlagNotFoundException : Exception
{
    /// <summary>Initializes a new instance of <see cref="FeatureFlagNotFoundException"/>.</summary>
    public FeatureFlagNotFoundException(string key)
        : base($"Feature flag '{key}' was not found.") { }
}

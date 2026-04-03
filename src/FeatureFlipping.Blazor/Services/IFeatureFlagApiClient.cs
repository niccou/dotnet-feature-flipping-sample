namespace FeatureFlipping.Blazor.Services;

/// <summary>HTTP client interface for the feature flag API.</summary>
public interface IFeatureFlagApiClient
{
    Task<List<FeatureFlagModel>> GetAllFlagsAsync();
    Task<FeatureFlagModel?> GetFlagAsync(string key);
    Task<FeatureFlagModel?> ToggleFlagAsync(string key);
    Task<FeatureFlagModel?> UpdateFlagAsync(string key, UpdateFlagRequest request);
    Task<FlagEvaluationModel?> EvaluateFlagAsync(string key, string? userId);
    Task<string> GetCacheStatusAsync(string key);
}

/// <summary>Model for a feature flag from the API.</summary>
public sealed record FeatureFlagModel(
    string Key,
    bool IsEnabled,
    string? Value,
    string[] UserTargeting,
    int RolloutPercentage,
    DateTime UpdatedAt
);

/// <summary>Request model for updating a flag.</summary>
public sealed record UpdateFlagRequest(
    bool IsEnabled,
    string? Value,
    string[]? UserTargeting,
    int RolloutPercentage
);

/// <summary>Model for flag evaluation result.</summary>
public sealed record FlagEvaluationModel(
    string Key,
    bool IsEnabled,
    string? Value,
    DateTime EvaluatedAt,
    string Reason
);

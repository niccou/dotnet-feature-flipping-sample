namespace FeatureFlipping.Application.DTOs;

/// <summary>Data transfer object for a feature flag.</summary>
public sealed record FeatureFlagDto(
    string Key,
    bool IsEnabled,
    string? Value,
    string[] UserTargeting,
    int RolloutPercentage,
    DateTime UpdatedAt
);

/// <summary>DTO for creating a feature flag.</summary>
public sealed record CreateFeatureFlagDto(
    string Key,
    bool IsEnabled = false,
    string? Value = null,
    string[]? UserTargeting = null,
    int RolloutPercentage = 0
);

/// <summary>DTO for updating a feature flag.</summary>
public sealed record UpdateFeatureFlagDto(
    bool IsEnabled,
    string? Value,
    string[]? UserTargeting,
    int RolloutPercentage
);

/// <summary>DTO for flag evaluation result.</summary>
public sealed record FlagEvaluationResultDto(
    string Key,
    bool IsEnabled,
    string? Value,
    DateTime EvaluatedAt,
    string Reason
);

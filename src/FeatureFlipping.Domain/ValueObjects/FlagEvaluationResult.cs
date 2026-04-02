namespace FeatureFlipping.Domain.ValueObjects;

/// <summary>Reason why a flag was evaluated to a particular result.</summary>
public enum EvaluationReason
{
    /// <summary>Flag was not found.</summary>
    NotFound,
    /// <summary>Flag is globally disabled.</summary>
    Disabled,
    /// <summary>Flag is enabled for everyone.</summary>
    Enabled,
    /// <summary>Flag is enabled because of user targeting.</summary>
    UserTargeted,
    /// <summary>Flag is enabled via rollout percentage.</summary>
    RolledOut
}

/// <summary>Value object representing the result of a flag evaluation.</summary>
public sealed record FlagEvaluationResult(
    bool IsEnabled,
    string? Value,
    DateTime EvaluatedAt,
    EvaluationReason Reason
)
{
    /// <summary>Creates a NotFound result.</summary>
    public static FlagEvaluationResult NotFound() =>
        new(false, null, DateTime.UtcNow, EvaluationReason.NotFound);
}

using FeatureFlipping.Domain.ValueObjects;

namespace FeatureFlipping.Domain.Aggregates;

/// <summary>Aggregate root representing a feature flag.</summary>
public sealed class FeatureFlag
{
    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    /// <summary>The unique key identifying this flag.</summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>Whether this flag is enabled.</summary>
    public bool IsEnabled { get; private set; }

    /// <summary>Optional typed configuration value (can hold int, string, JSON).</summary>
    public string? Value { get; private set; }

    /// <summary>Comma-separated list of user IDs or roles targeted by this flag.</summary>
    public string? UserTargeting { get; private set; }

    /// <summary>Gradual rollout percentage (0–100).</summary>
    public int RolloutPercentage { get; private set; }

    /// <summary>When this flag was last updated.</summary>
    public DateTime UpdatedAt { get; private set; }

    private FeatureFlag() { } // EF Core

    /// <summary>Creates a new feature flag.</summary>
    public static FeatureFlag Create(FlagKey key, bool isEnabled = false, string? value = null,
        string[]? userTargeting = null, int rolloutPercentage = 0)
    {
        return new FeatureFlag
        {
            Key = key.Value,
            IsEnabled = isEnabled,
            Value = value,
            UserTargeting = userTargeting != null && userTargeting.Length > 0 ? string.Join(",", userTargeting) : null,
            RolloutPercentage = Math.Clamp(rolloutPercentage, 0, 100),
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>Toggles IsEnabled.</summary>
    public void Toggle()
    {
        IsEnabled = !IsEnabled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Updates the flag properties.</summary>
    public void Update(bool isEnabled, string? value, string[]? userTargeting, int rolloutPercentage)
    {
        IsEnabled = isEnabled;
        Value = value;
        UserTargeting = userTargeting != null && userTargeting.Length > 0 ? string.Join(",", userTargeting) : null;
        RolloutPercentage = Math.Clamp(rolloutPercentage, 0, 100);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets user targeting as an array.</summary>
    public string[] GetUserTargeting() =>
        string.IsNullOrEmpty(UserTargeting) ? [] : UserTargeting.Split(',', StringSplitOptions.RemoveEmptyEntries);
}

using System.Text.RegularExpressions;

namespace FeatureFlipping.Domain.ValueObjects;

/// <summary>Value object representing a validated, lowercase-kebab flag key.</summary>
public sealed record FlagKey
{
    private static readonly Regex ValidPattern = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    /// <summary>The raw string value of the flag key.</summary>
    public string Value { get; }

    /// <summary>Initializes a new instance of <see cref="FlagKey"/>.</summary>
    public FlagKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exceptions.InvalidFlagKeyException("Flag key cannot be empty.");
        var normalized = value.Trim().ToLowerInvariant();
        if (!ValidPattern.IsMatch(normalized))
            throw new Exceptions.InvalidFlagKeyException($"Flag key '{value}' is not a valid lowercase-kebab identifier.");
        Value = normalized;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>Implicit conversion from string.</summary>
    public static implicit operator string(FlagKey key) => key.Value;
}

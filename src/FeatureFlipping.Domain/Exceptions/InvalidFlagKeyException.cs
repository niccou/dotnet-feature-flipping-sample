namespace FeatureFlipping.Domain.Exceptions;

/// <summary>Thrown when a flag key is invalid.</summary>
public sealed class InvalidFlagKeyException : Exception
{
    /// <summary>Initializes a new instance of <see cref="InvalidFlagKeyException"/>.</summary>
    public InvalidFlagKeyException(string message) : base(message) { }
}

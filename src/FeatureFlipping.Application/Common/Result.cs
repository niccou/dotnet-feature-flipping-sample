namespace FeatureFlipping.Application.Common;

/// <summary>Represents the result of an operation that can fail with an error message.</summary>
public sealed class Result<T>
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>The value if success.</summary>
    public T? Value { get; }

    /// <summary>The error message if failure.</summary>
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Creates a successful result.</summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>Creates a failure result.</summary>
    public static Result<T> Failure(string error) => new(false, default, error);
}

/// <summary>Non-generic result for operations that return no value.</summary>
public sealed class Result
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>The error message if failure.</summary>
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, null);

    /// <summary>Creates a failure result.</summary>
    public static Result Failure(string error) => new(false, error);
}

namespace Gorgeous.Abstractions.Results;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type = ErrorType.Failure,
    ErrorVisibility Visibility = ErrorVisibility.Sensitive)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
}

public enum ErrorVisibility
{
    Sensitive,
    Public
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    TooManyRequests
}

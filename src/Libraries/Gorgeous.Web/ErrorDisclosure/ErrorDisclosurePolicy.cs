using Gorgeous.Abstractions.Results;

namespace Gorgeous.Web.ErrorDisclosure;

public sealed class ErrorDisclosurePolicy
{
    private readonly IReadOnlyDictionary<string, PublicError> _exactMasks;
    private readonly IReadOnlyList<PrefixMask> _prefixMasks;

    private ErrorDisclosurePolicy()
    {
        _exactMasks = new Dictionary<string, PublicError>(StringComparer.Ordinal);
        _prefixMasks = [];
    }

    private ErrorDisclosurePolicy(
        IReadOnlyDictionary<string, PublicError> exactMasks,
        IReadOnlyList<PrefixMask> prefixMasks)
    {
        _exactMasks = exactMasks;
        _prefixMasks = prefixMasks;
    }

    public static ErrorDisclosurePolicy Create() => new();

    public ErrorDisclosurePolicy Mask(
        IEnumerable<string> errorCodes,
        string publicCode,
        string publicMessage,
        ErrorType publicType)
    {
        ArgumentNullException.ThrowIfNull(errorCodes);

        var publicError = CreatePublicError(publicCode, publicMessage, publicType);
        var exactMasks = new Dictionary<string, PublicError>(_exactMasks, StringComparer.Ordinal);
        bool hasErrorCode = false;

        foreach (string errorCode in errorCodes)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                throw new ArgumentException("Error codes cannot contain empty values.", nameof(errorCodes));
            }

            hasErrorCode = true;
            exactMasks[errorCode] = publicError;
        }

        if (!hasErrorCode)
        {
            throw new ArgumentException("At least one error code is required.", nameof(errorCodes));
        }

        return new ErrorDisclosurePolicy(exactMasks, _prefixMasks);
    }

    public ErrorDisclosurePolicy MaskPrefix(
        string errorCodePrefix,
        string publicCode,
        string publicMessage,
        ErrorType publicType)
    {
        if (string.IsNullOrWhiteSpace(errorCodePrefix))
        {
            throw new ArgumentException("Error code prefix is required.", nameof(errorCodePrefix));
        }

        var prefixMasks = _prefixMasks
            .Append(new PrefixMask(
                errorCodePrefix,
                CreatePublicError(publicCode, publicMessage, publicType)))
            .ToArray();

        return new ErrorDisclosurePolicy(_exactMasks, prefixMasks);
    }

    public bool TryMask(Error error, out PublicError publicError)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (_exactMasks.TryGetValue(error.Code, out publicError!))
        {
            return true;
        }

        foreach (var prefixMask in _prefixMasks)
        {
            if (error.Code.StartsWith(prefixMask.ErrorCodePrefix, StringComparison.Ordinal))
            {
                publicError = prefixMask.PublicError;
                return true;
            }
        }

        publicError = null!;
        return false;
    }

    private static PublicError CreatePublicError(string code, string message, ErrorType type)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Public error code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Public error message is required.", nameof(message));
        }

        return new PublicError(code, message, type);
    }

    private sealed record PrefixMask(string ErrorCodePrefix, PublicError PublicError);
}

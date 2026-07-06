using Gorgeous.Abstractions.Results;
using Gorgeous.Web.ErrorDisclosure;
using Microsoft.AspNetCore.Http;

namespace Gorgeous.Web;

public static class EndpointResults
{
    private static readonly ErrorDisclosurePolicy EmptyDisclosurePolicy = ErrorDisclosurePolicy.Create();
    private static readonly PublicError GenericPublicErrorFallback = new(
        "Common.RequestFailed",
        "Request could not be completed.",
        ErrorType.Failure);

    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ToProblem(result.Error);
    }

    public static IResult ToHttpResult<TValue>(
        this Result<TValue> result,
        Func<TValue, IResult>? onSuccess = null)
    {
        if (result.IsFailure)
        {
            return ToProblem(result.Error);
        }

        return onSuccess is null
            ? TypedResults.Ok(result.Value)
            : onSuccess(result.Value);
    }

    public static IResult ToPublicHttpResult(this Result result)
    {
        return result.ToPublicHttpResult(EmptyDisclosurePolicy);
    }

    public static IResult ToPublicHttpResult(this Result result, ErrorDisclosurePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : ToProblem(Disclose(result.Error, policy));
    }

    public static IResult ToPublicHttpResult<TValue>(
        this Result<TValue> result,
        Func<TValue, IResult>? onSuccess = null)
    {
        return result.ToPublicHttpResult(EmptyDisclosurePolicy, onSuccess);
    }

    public static IResult ToPublicHttpResult<TValue>(
        this Result<TValue> result,
        ErrorDisclosurePolicy policy,
        Func<TValue, IResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (result.IsFailure)
        {
            return ToProblem(Disclose(result.Error, policy));
        }

        return onSuccess is null
            ? TypedResults.Ok(result.Value)
            : onSuccess(result.Value);
    }

    public static IResult ToProblem(Error error)
    {
        int statusCode = error.Type.ToStatusCode();

        return TypedResults.Problem(
            detail: error.Message,
            statusCode: statusCode,
            title: GetTitle(error.Type),
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code
            });
    }

    public static IResult ToProblem(PublicError publicError)
    {
        int statusCode = publicError.Type.ToStatusCode();

        return TypedResults.Problem(
            detail: publicError.Message,
            statusCode: statusCode,
            title: GetTitle(publicError.Type),
            extensions: new Dictionary<string, object?>
            {
                ["code"] = publicError.Code
            });
    }

    private static PublicError Disclose(Error error, ErrorDisclosurePolicy policy)
    {
        if (policy.TryMask(error, out var publicError))
        {
            return publicError;
        }

        if (error.Visibility == ErrorVisibility.Public)
        {
            return new PublicError(error.Code, error.Message, error.Type);
        }

        return GenericPublicErrorFallback with { Type = error.Type };
    }

    private static int ToStatusCode(this ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => "Validation error",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.NotFound => "Not found",
            ErrorType.Conflict => "Conflict",
            ErrorType.TooManyRequests => "Too many requests",
            _ => "Server error"
        };
    }
}

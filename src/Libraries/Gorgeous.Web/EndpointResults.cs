using Gorgeous.Abstractions.Results;
using Microsoft.AspNetCore.Http;

namespace Gorgeous.Web;

public static class EndpointResults
{
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


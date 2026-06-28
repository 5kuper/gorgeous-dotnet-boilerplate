using Shared.BuildingBlocks.Core.Results;

namespace Shared.TestKit.Assertions;

public static class ResultAssertions
{
    public static void ShouldSucceed(this Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Expected success, but result failed with '{result.Error.Code}'.");
        }

        if (result.Error != Error.None)
        {
            throw new InvalidOperationException($"Expected no error, but result had '{result.Error.Code}'.");
        }
    }

    public static TValue ShouldSucceed<TValue>(this Result<TValue> result)
    {
        ShouldSucceed((Result)result);

        return result.Value;
    }

    public static void ShouldFailWith(this Result result, Error expectedError)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException($"Expected failure '{expectedError.Code}', but result succeeded.");
        }

        if (result.Error != expectedError)
        {
            throw new InvalidOperationException(
                $"Expected failure '{expectedError.Code}', but result failed with '{result.Error.Code}'.");
        }
    }

    public static void ShouldFailWith<TValue>(this Result<TValue> result, Error expectedError)
    {
        ShouldFailWith((Result)result, expectedError);
    }
}

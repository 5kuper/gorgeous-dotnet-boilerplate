using Auth.Application.Ports.Registration;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeRegistrationTransaction : IRegistrationTransaction
{
    public int ExecuteCalls { get; private set; }

    public Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<Result<TResponse>>> operation,
        CancellationToken ct = default)
    {
        ExecuteCalls++;

        return operation(ct);
    }
}

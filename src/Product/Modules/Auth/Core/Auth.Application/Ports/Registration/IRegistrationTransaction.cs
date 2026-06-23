using Shared.BuildingBlocks.Core.Results;

namespace Auth.Application.Ports.Registration;

public interface IRegistrationTransaction
{
    Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<Result<TResponse>>> operation,
        CancellationToken ct = default);
}


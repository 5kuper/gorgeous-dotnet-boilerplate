using Shared.BuildingBlocks.Core.Results;

namespace Users.Contracts.Registration;

public interface IUsersRegistration
{
    Task<Result<RegisteredUser>> RegisterAsync(
        RegisterUser request,
        CancellationToken ct = default);
}


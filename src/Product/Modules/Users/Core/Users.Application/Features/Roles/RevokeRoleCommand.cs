using Shared.BuildingBlocks.Application.Abstractions;
using Shared.BuildingBlocks.Application.Messaging;
using Shared.BuildingBlocks.Core.Results;
using Users.Domain.Entities;
using Users.Domain.Foundation.Errors;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles;

public sealed record RevokeRoleCommand(Guid UserPublicId, string RoleCode) : ICommand;

internal sealed class RevokeRoleCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RevokeRoleCommand>
{
    public async Task<Result> Handle(RevokeRoleCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByPublicIdAsync(request.UserPublicId, ct);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var role = await roleRepository.GetByCodeAsync(Role.NormalizeCode(request.RoleCode), ct);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        var result = user.RevokeRole(role.Id, clock.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

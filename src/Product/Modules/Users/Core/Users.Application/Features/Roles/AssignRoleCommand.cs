using Gorgeous.Abstractions.Application;
using Shared.AppModel.Abstractions;
using Shared.AppModel.Messaging;
using Gorgeous.Abstractions.Results;
using Users.Domain.Entities;
using Users.Domain.Foundation.Errors;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles;

public sealed record AssignRoleCommand(Guid UserPublicId, string RoleCode, Guid AssignedByUserPublicId) : ICommand;

internal sealed class AssignRoleCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AssignRoleCommand>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken ct)
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

        var assignedByUser = await userRepository.GetByPublicIdAsync(request.AssignedByUserPublicId, ct);

        if (assignedByUser is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var result = user.AssignRole(role.Id, clock.UtcNow, assignedByUser.Id);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

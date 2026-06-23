using Shared.BuildingBlocks.Application.Abstractions;
using Shared.BuildingBlocks.Application.Messaging;
using Shared.BuildingBlocks.Core.Results;
using Users.Domain.Entities;
using Users.Domain.Foundation.Errors;
using Users.Domain.Repositories;

namespace Users.Application.Features.Roles;

public sealed record CreateRoleCommand(string Code, string Name, string? Description, bool IsSystem) : ICommand<long>;

internal sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateRoleCommand, long>
{
    public async Task<Result<long>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        string code = Role.NormalizeCode(request.Code);

        if (await roleRepository.GetByCodeAsync(code, ct) is not null)
        {
            return Result.Failure<long>(RoleErrors.CodeAlreadyUsed);
        }

        var result = Role.Create(code, request.Name, request.Description, request.IsSystem, clock.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<long>(result.Error);
        }

        await roleRepository.AddAsync(result.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(result.Value.Id);
    }
}

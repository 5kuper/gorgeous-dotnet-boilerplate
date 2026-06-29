using Gorgeous.Abstractions.Application;
using Shared.AppModel.Abstractions;
using Shared.AppModel.Messaging;
using Gorgeous.Abstractions.Results;
using Users.Contracts.Registration;
using Users.Domain.Entities;
using Users.Domain.Foundation.Errors;
using Users.Domain.Foundation.Primitives;
using Users.Domain.Repositories;

namespace Users.Application.Features.Registration;

public sealed record RegisterUserCommand(
    string? Email,
    string DisplayName,
    string RegistrationMethod,
    bool RequireEmailConfirmation) : ICommand<RegisteredUser>;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RegisterUserCommand, RegisteredUser>
{
    private const string DefaultRoleCode = "user";

    public async Task<Result<RegisteredUser>> Handle(
        RegisterUserCommand request,
        CancellationToken ct)
    {
        if (!Enum.TryParse(request.RegistrationMethod, ignoreCase: true, out RegistrationMethod registrationMethod))
        {
            return Result.Failure<RegisteredUser>(new Error(
                "Users.InvalidRegistrationMethod",
                "Registration method is invalid.",
                ErrorType.Validation));
        }

        string? normalizedEmail = User.NormalizeEmail(request.Email);

        if (normalizedEmail is not null
            && await userRepository.GetByEmailAsync(normalizedEmail, ct) is not null)
        {
            return Result.Failure<RegisteredUser>(UserErrors.EmailAlreadyUsed);
        }

        var nowUtc = clock.UtcNow;
        var userResult = User.Register(
            request.Email,
            request.DisplayName,
            registrationMethod,
            request.RequireEmailConfirmation,
            nowUtc);

        if (userResult.IsFailure)
        {
            return Result.Failure<RegisteredUser>(userResult.Error);
        }

        var defaultRole = await roleRepository.GetByCodeAsync(DefaultRoleCode, ct);

        if (defaultRole is null)
        {
            return Result.Failure<RegisteredUser>(RoleErrors.NotFound);
        }

        var user = userResult.Value;
        var assignRoleResult = user.AssignRole(defaultRole.Id, nowUtc);

        if (assignRoleResult.IsFailure)
        {
            return Result.Failure<RegisteredUser>(assignRoleResult.Error);
        }

        await userRepository.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new RegisteredUser(
            user.Id,
            user.PublicId,
            user.Email,
            user.DisplayName,
            user.Status.ToString()));
    }
}

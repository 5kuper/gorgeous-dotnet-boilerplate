using Shared.Kernel.BuildingBlocks;
using Gorgeous.Abstractions.Results;
using Users.Domain.Foundation.Errors;
using Users.Domain.Foundation.Events;
using Users.Domain.Foundation.Primitives;

namespace Users.Domain.Entities;

public sealed class User : AggregateRoot<long>
{
    private readonly List<UserRole> _roles = [];

    private User()
    {
    }

    private User(
        Guid publicId,
        string? email,
        string displayName,
        UserStatus status,
        bool emailVerified,
        RegistrationMethod registrationMethod,
        DateTime nowUtc)
    {
        PublicId = publicId;
        Email = NormalizeEmail(email);
        DisplayName = displayName.Trim();
        Status = status;
        EmailVerified = emailVerified;
        RegistrationMethod = registrationMethod;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        Raise(new UserRegisteredDomainEvent(Id, publicId, nowUtc));
    }

    public Guid PublicId { get; private set; }

    public string? Email { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    public bool EmailVerified { get; private set; }

    public RegistrationMethod RegistrationMethod { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;

    public static Result<User> Register(
        string? email,
        string displayName,
        RegistrationMethod registrationMethod,
        bool requireEmailConfirmation,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<User>(UserErrors.DisplayNameRequired);
        }

        if (email is not null && string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(UserErrors.InvalidEmail);
        }

        bool emailVerified = registrationMethod is not RegistrationMethod.EmailPassword || !requireEmailConfirmation;
        var status = ResolveInitialStatus(registrationMethod, requireEmailConfirmation);

        return Result.Success(new User(
            Guid.NewGuid(),
            email,
            displayName,
            status,
            emailVerified,
            registrationMethod,
            nowUtc));
    }

    public Result ConfirmEmail(DateTime nowUtc)
    {
        EmailVerified = true;

        if (RegistrationMethod == RegistrationMethod.EmailPassword && Status == UserStatus.PendingActivation)
        {
            Status = UserStatus.Active;
        }

        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result UpdateProfile(string displayName, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure(UserErrors.DisplayNameRequired);
        }

        DisplayName = displayName.Trim();
        UpdatedAtUtc = nowUtc;

        return Result.Success();
    }

    public Result Suspend(DateTime nowUtc)
    {
        if (Status == UserStatus.Archived)
        {
            return Result.Failure(UserErrors.CannotAuthenticate);
        }

        Status = UserStatus.Suspended;
        UpdatedAtUtc = nowUtc;
        Raise(new UserSuspendedDomainEvent(Id, nowUtc));

        return Result.Success();
    }

    public Result AssignRole(long roleId, DateTime nowUtc, long? assignedByUserId = null)
    {
        if (_roles.Any(role => role.RoleId == roleId))
        {
            return Result.Failure(UserErrors.RoleAlreadyAssigned);
        }

        _roles.Add(new UserRole(roleId, nowUtc, assignedByUserId));
        UpdatedAtUtc = nowUtc;
        Raise(new UserRoleAssignedDomainEvent(Id, roleId, nowUtc));

        return Result.Success();
    }

    public Result RevokeRole(long roleId, DateTime nowUtc)
    {
        var userRole = _roles.FirstOrDefault(role => role.RoleId == roleId);

        if (userRole is null)
        {
            return Result.Failure(UserErrors.RoleNotAssigned);
        }

        _roles.Remove(userRole);
        UpdatedAtUtc = nowUtc;
        Raise(new UserRoleRevokedDomainEvent(Id, roleId, nowUtc));

        return Result.Success();
    }

    public bool CanAuthenticate()
    {
        return Status == UserStatus.Active;
    }

    public static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToUpperInvariant();
    }

    private static UserStatus ResolveInitialStatus(
        RegistrationMethod registrationMethod,
        bool requireEmailConfirmation)
    {
        return registrationMethod == RegistrationMethod.EmailPassword && requireEmailConfirmation
            ? UserStatus.PendingActivation
            : UserStatus.Active;
    }
}

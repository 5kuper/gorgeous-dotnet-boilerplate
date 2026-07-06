using Gorgeous.Abstractions.Results;

namespace Users.Domain.Foundation.Errors;

public static class RoleErrors
{
    public static readonly Error NotFound = new(
        "Users.RoleNotFound",
        "Role was not found.",
        ErrorType.NotFound);

    public static readonly Error CodeRequired = new(
        "Users.RoleCodeRequired",
        "Role code is required.",
        ErrorType.Validation,
        ErrorVisibility.Public);

    public static readonly Error NameRequired = new(
        "Users.RoleNameRequired",
        "Role name is required.",
        ErrorType.Validation,
        ErrorVisibility.Public);

    public static readonly Error CodeAlreadyUsed = new(
        "Users.RoleCodeAlreadyUsed",
        "Role code is already used.",
        ErrorType.Conflict);

    public static readonly Error SystemRoleCannotBeDeleted = new(
        "Users.SystemRoleCannotBeDeleted",
        "System role cannot be deleted.",
        ErrorType.Forbidden);
}

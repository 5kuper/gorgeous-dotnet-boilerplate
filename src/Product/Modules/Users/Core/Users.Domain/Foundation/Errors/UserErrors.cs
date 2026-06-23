using Shared.BuildingBlocks.Core.Results;

namespace Users.Domain.Foundation.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new(
        "Users.NotFound",
        "User was not found.",
        ErrorType.NotFound);

    public static readonly Error EmailAlreadyUsed = new(
        "Users.EmailAlreadyUsed",
        "Email is already used.",
        ErrorType.Conflict);

    public static readonly Error InvalidEmail = new(
        "Users.InvalidEmail",
        "Email is invalid.",
        ErrorType.Validation);

    public static readonly Error DisplayNameRequired = new(
        "Users.DisplayNameRequired",
        "Display name is required.",
        ErrorType.Validation);

    public static readonly Error RoleAlreadyAssigned = new(
        "Users.RoleAlreadyAssigned",
        "Role is already assigned to user.",
        ErrorType.Conflict);

    public static readonly Error RoleNotAssigned = new(
        "Users.RoleNotAssigned",
        "Role is not assigned to user.",
        ErrorType.NotFound);

    public static readonly Error CannotAuthenticate = new(
        "Users.CannotAuthenticate",
        "User is not allowed to authenticate.",
        ErrorType.Forbidden);
}

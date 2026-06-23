using Shared.BuildingBlocks.Core.Results;

namespace Auth.Domain.Foundation.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "Invalid credentials.",
        ErrorType.Unauthorized);

    public static readonly Error UserNotAllowed = new(
        "Auth.UserNotAllowed",
        "User is not allowed to sign in.",
        ErrorType.Forbidden);

    public static readonly Error RefreshSessionNotFound = new(
        "Auth.RefreshSessionNotFound",
        "Refresh session was not found.",
        ErrorType.Unauthorized);

    public static readonly Error RefreshSessionExpired = new(
        "Auth.RefreshSessionExpired",
        "Refresh session is expired.",
        ErrorType.Unauthorized);

    public static readonly Error RefreshSessionRevoked = new(
        "Auth.RefreshSessionRevoked",
        "Refresh session is revoked.",
        ErrorType.Unauthorized);

    public static readonly Error RefreshSessionReplayed = new(
        "Auth.RefreshSessionReplayed",
        "Refresh session replay was detected.",
        ErrorType.Unauthorized);

    public static readonly Error EmailNotConfirmed = new(
        "Auth.EmailNotConfirmed",
        "Email is not confirmed.",
        ErrorType.Forbidden);

    public static readonly Error PasswordResetFailed = new(
        "Auth.PasswordResetFailed",
        "Password reset failed.",
        ErrorType.Validation);

    public static readonly Error InvalidEmailConfirmationCode = new(
        "Auth.InvalidEmailConfirmationCode",
        "Email confirmation code is invalid.",
        ErrorType.Validation);

    public static readonly Error IdentityOperationFailed = new(
        "Auth.IdentityOperationFailed",
        "Identity operation failed.",
        ErrorType.Failure);
}

using Gorgeous.Abstractions.Results;
using Gorgeous.Web.ErrorDisclosure;

namespace Auth.Presentation.WebApi;

public static class AuthErrorDisclosure
{
    public static readonly ErrorDisclosurePolicy Register =
        ErrorDisclosurePolicy.Create()
            .Mask(
                ["Users.EmailAlreadyUsed", "Auth.IdentityOperationFailed"],
                "Auth.RegistrationFailed",
                "Registration could not be completed.",
                ErrorType.Validation);

    public static readonly ErrorDisclosurePolicy Login =
        ErrorDisclosurePolicy.Create()
            .Mask(
                ["Auth.InvalidCredentials", "Auth.UserNotAllowed", "Auth.EmailNotConfirmed"],
                "Auth.LoginFailed",
                "Invalid email or password.",
                ErrorType.Unauthorized);

    public static readonly ErrorDisclosurePolicy Refresh =
        ErrorDisclosurePolicy.Create()
            .MaskPrefix(
                "Auth.RefreshSession",
                "Auth.RefreshFailed",
                "Refresh token is invalid.",
                ErrorType.Unauthorized);

    public static readonly ErrorDisclosurePolicy EmailConfirmation =
        ErrorDisclosurePolicy.Create()
            .Mask(
                ["Auth.UserNotAllowed", "Users.NotFound", "Auth.IdentityOperationFailed"],
                "Auth.EmailConfirmationFailed",
                "Email confirmation failed.",
                ErrorType.Validation);
}

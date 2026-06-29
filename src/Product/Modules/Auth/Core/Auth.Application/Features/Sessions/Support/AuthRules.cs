using Auth.Domain.Foundation.Errors;
using Gorgeous.Abstractions.Results;
using Users.Contracts.Authentication;

namespace Auth.Application.Features.Sessions.Support;

internal static class AuthRules
{
    public const bool RequireConfirmedEmailForPasswordLogin = true;
    public const int RefreshTokenDays = 30;

    public static Result EnsureCanAuthenticate(UserAuthProfile profile)
    {
        if (!string.Equals(profile.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(AuthErrors.UserNotAllowed);
        }

        if (RequireConfirmedEmailForPasswordLogin && !profile.EmailVerified)
        {
            return Result.Failure(AuthErrors.EmailNotConfirmed);
        }

        return Result.Success();
    }
}

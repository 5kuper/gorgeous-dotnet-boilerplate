using Microsoft.Extensions.Options;

namespace Auth.Infrastructure.Identity;

internal sealed class AuthIdentityOptionsValidator : IValidateOptions<AuthIdentityOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthIdentityOptions options)
    {
        if (options.MaxFailedAccessAttempts is < 1 or > 100)
        {
            return ValidateOptionsResult.Fail(
                $"{AuthIdentityOptions.SectionName}:MaxFailedAccessAttempts must be between 1 and 100.");
        }

        return ValidateOptionsResult.Success;
    }
}

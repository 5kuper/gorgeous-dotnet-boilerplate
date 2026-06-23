using System.Text;
using Microsoft.Extensions.Options;

namespace Auth.Infrastructure.Tokens;

internal sealed class AuthTokenOptionsValidator : IValidateOptions<AuthTokenOptions>
{
    private static readonly string[] PlaceholderMarkers = ["CHANGE_ME", "TODO", "PLACEHOLDER"];

    public ValidateOptionsResult Validate(string? name, AuthTokenOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add($"{AuthTokenOptions.SectionName}:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add($"{AuthTokenOptions.SectionName}:Audience is required.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            failures.Add($"{AuthTokenOptions.SectionName}:SigningKey is required.");
        }
        else
        {
            if (PlaceholderMarkers.Any(marker =>
                    options.SigningKey.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            {
                failures.Add($"{AuthTokenOptions.SectionName}:SigningKey must not contain a placeholder value.");
            }

            if (Encoding.UTF8.GetByteCount(options.SigningKey) < AuthTokenOptions.MinimumSigningKeyBytes)
            {
                failures.Add($"{AuthTokenOptions.SectionName}:SigningKey must be at least {AuthTokenOptions.MinimumSigningKeyBytes} UTF-8 bytes.");
            }
        }

        if (options.AccessTokenMinutes is < 1 or > 1440)
        {
            failures.Add($"{AuthTokenOptions.SectionName}:AccessTokenMinutes must be between 1 and 1440.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

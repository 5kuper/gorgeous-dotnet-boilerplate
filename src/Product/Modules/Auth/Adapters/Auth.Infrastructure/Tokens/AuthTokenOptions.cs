namespace Auth.Infrastructure.Tokens;

public sealed class AuthTokenOptions
{
    public const string SectionName = "Auth:Jwt";

    public const int MinimumSigningKeyBytes = 32;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 10;
}

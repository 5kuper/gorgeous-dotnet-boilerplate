namespace Auth.Infrastructure.Identity;

public sealed class AuthIdentityOptions
{
    public const string SectionName = "Auth:Identity";

    public bool RequireUniqueEmail { get; init; } = true;

    public bool RequireConfirmedEmail { get; init; } = true;

    public bool LockoutAllowedForNewUsers { get; init; } = true;

    public int MaxFailedAccessAttempts { get; init; } = 5;
}

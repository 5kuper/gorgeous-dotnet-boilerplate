namespace Users.Contracts.Authentication;

public sealed record UserAuthProfile(
    long UserId,
    Guid PublicId,
    string? Email,
    string Status,
    bool EmailVerified,
    IReadOnlyCollection<string> Roles);

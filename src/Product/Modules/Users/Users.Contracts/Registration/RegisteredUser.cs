namespace Users.Contracts.Registration;

public sealed record RegisteredUser(
    long UserId,
    Guid PublicId,
    string? Email,
    string DisplayName,
    string Status);

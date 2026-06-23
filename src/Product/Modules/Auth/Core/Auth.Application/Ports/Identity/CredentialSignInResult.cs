namespace Auth.Application.Ports.Identity;

public sealed record CredentialSignInResult(
    bool Succeeded,
    bool IsNotAllowed,
    long? UserId);

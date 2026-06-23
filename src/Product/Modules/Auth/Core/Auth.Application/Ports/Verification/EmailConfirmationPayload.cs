namespace Auth.Application.Ports.Verification;

public sealed record EmailConfirmationPayload(
    Guid UserPublicId,
    string Token);

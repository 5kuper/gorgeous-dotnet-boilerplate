namespace Auth.Application.Features.Registration;

public sealed record RegistrationResult(
    Guid UserPublicId,
    string? Email,
    bool EmailConfirmationRequired);

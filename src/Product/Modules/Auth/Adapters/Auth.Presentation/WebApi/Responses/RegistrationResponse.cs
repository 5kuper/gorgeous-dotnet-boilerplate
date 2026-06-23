namespace Auth.Presentation.WebApi.Responses;

internal sealed record RegistrationResponse(
    Guid UserPublicId,
    string? Email,
    bool EmailConfirmationRequired);

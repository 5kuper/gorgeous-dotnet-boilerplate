namespace Users.Contracts.Registration;

public sealed record RegisterUser(
    string? Email,
    string DisplayName,
    string RegistrationMethod,
    bool RequireEmailConfirmation);

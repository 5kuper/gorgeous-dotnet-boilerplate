namespace Auth.Presentation.WebApi.Responses;

internal sealed record AuthSessionResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    Guid UserPublicId);

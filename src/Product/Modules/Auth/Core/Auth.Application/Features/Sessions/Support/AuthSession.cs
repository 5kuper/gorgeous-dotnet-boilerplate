namespace Auth.Application.Features.Sessions.Support;

public sealed record AuthSession(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    Guid UserPublicId);

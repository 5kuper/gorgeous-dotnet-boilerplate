namespace Auth.Application.Ports.Tokens;

public sealed record IssuedAccessToken(string Token, DateTime ExpiresAtUtc);

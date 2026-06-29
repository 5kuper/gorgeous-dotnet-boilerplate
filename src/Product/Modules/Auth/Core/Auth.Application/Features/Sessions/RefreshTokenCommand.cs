using Auth.Application.Features.Sessions.Support;
using Auth.Application.Ports.Tokens;
using Auth.Domain.Entities;
using Auth.Domain.Foundation.Errors;
using Auth.Domain.Repositories;
using Gorgeous.Abstractions.Application;
using Shared.AppModel.Messaging;
using Gorgeous.Abstractions.Results;
using Users.Contracts.Authentication;

namespace Auth.Application.Features.Sessions;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? DeviceName,
    string? IpAddress) : ICommand<AuthSession>;

internal sealed class RefreshTokenCommandHandler(
    IRefreshSessionRepository refreshSessionRepository,
    IUserAuthProfileReader userAuthProfileReader,
    ITokenIssuer tokenIssuer,
    IClock clock) : ICommandHandler<RefreshTokenCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(
        RefreshTokenCommand request,
        CancellationToken ct)
    {
        string tokenHash = TokenHashing.Sha256(request.RefreshToken);
        var session = await refreshSessionRepository.GetByTokenHashAsync(tokenHash, ct);

        if (session is null)
        {
            return Result.Failure<AuthSession>(AuthErrors.RefreshSessionNotFound);
        }

        var nowUtc = clock.UtcNow;
        var sessionResult = session.EnsureCanRefresh(nowUtc);

        if (sessionResult.IsFailure)
        {
            if (sessionResult.Error == AuthErrors.RefreshSessionReplayed)
            {
                await refreshSessionRepository.RevokeAllActiveSessionsForUserAsync(
                    session.UserId,
                    nowUtc,
                    ct);
                await refreshSessionRepository.SaveChangesAsync(ct);
            }

            return Result.Failure<AuthSession>(sessionResult.Error);
        }

        var profile = await userAuthProfileReader.GetByUserIdAsync(session.UserId, ct);

        if (profile is null)
        {
            return Result.Failure<AuthSession>(AuthErrors.UserNotAllowed);
        }

        var authResult = AuthRules.EnsureCanAuthenticate(profile);

        if (authResult.IsFailure)
        {
            return Result.Failure<AuthSession>(authResult.Error);
        }

        string newRefreshToken = TokenHashing.GenerateToken();
        var refreshExpiresAtUtc = nowUtc.AddDays(AuthRules.RefreshTokenDays);
        var newSession = RefreshSession.Create(
            session.UserId,
            TokenHashing.Sha256(newRefreshToken),
            refreshExpiresAtUtc,
            nowUtc,
            request.DeviceName ?? session.DeviceName,
            request.IpAddress ?? session.IpAddress);

        await refreshSessionRepository.RotateAsync(session, newSession, nowUtc, ct);

        var accessToken = tokenIssuer.IssueAccessToken(profile, newSession.Id);

        return Result.Success(new AuthSession(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshToken,
            refreshExpiresAtUtc,
            profile.PublicId));
    }
}

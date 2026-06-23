using Auth.Application.Features.Sessions.Support;
using Auth.Application.Ports.Identity;
using Auth.Application.Ports.Tokens;
using Auth.Domain.Entities;
using Auth.Domain.Foundation.Errors;
using Auth.Domain.Repositories;
using Shared.BuildingBlocks.Application.Abstractions;
using Shared.BuildingBlocks.Application.Messaging;
using Shared.BuildingBlocks.Core.Results;
using Users.Contracts.Authentication;

namespace Auth.Application.Features.Sessions;

public sealed record LoginWithPasswordCommand(
    string Email,
    string Password,
    string? DeviceName,
    string? IpAddress) : ICommand<AuthSession>;

internal sealed class LoginWithPasswordCommandHandler(
    IIdentityCredentialService identityCredentialService,
    IUserAuthProfileReader userAuthProfileReader,
    IRefreshSessionRepository refreshSessionRepository,
    ITokenIssuer tokenIssuer,
    IClock clock) : ICommandHandler<LoginWithPasswordCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(
        LoginWithPasswordCommand request,
        CancellationToken ct)
    {
        var signInResult = await identityCredentialService.CheckPasswordSignInByEmailAsync(
            request.Email,
            request.Password,
            ct);

        if (!signInResult.Succeeded || signInResult.UserId is null)
        {
            return Result.Failure<AuthSession>(
                signInResult.IsNotAllowed ? AuthErrors.UserNotAllowed : AuthErrors.InvalidCredentials);
        }

        var profile = await userAuthProfileReader.GetByUserIdAsync(signInResult.UserId.Value, ct);

        if (profile is null)
        {
            return Result.Failure<AuthSession>(AuthErrors.UserNotAllowed);
        }

        var authResult = AuthRules.EnsureCanAuthenticate(profile);

        if (authResult.IsFailure)
        {
            return Result.Failure<AuthSession>(authResult.Error);
        }

        var nowUtc = clock.UtcNow;
        string refreshToken = TokenHashing.GenerateToken();
        string tokenHash = TokenHashing.Sha256(refreshToken);
        var refreshExpiresAtUtc = nowUtc.AddDays(AuthRules.RefreshTokenDays);

        var session = RefreshSession.Create(
            profile.UserId,
            tokenHash,
            refreshExpiresAtUtc,
            nowUtc,
            request.DeviceName,
            request.IpAddress);

        await refreshSessionRepository.AddAsync(session, ct);
        await refreshSessionRepository.SaveChangesAsync(ct);

        var accessToken = tokenIssuer.IssueAccessToken(profile, session.Id);

        return Result.Success(new AuthSession(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken,
            refreshExpiresAtUtc,
            profile.PublicId));
    }
}

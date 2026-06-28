using Auth.Application.Features.Sessions;
using Auth.CoreTests.Support.Builders;
using Auth.CoreTests.Support.Fixtures;
using Auth.Domain.Entities;
using Auth.Domain.Foundation.Errors;
using Shared.TestKit.Assertions;
using Users.Contracts.Authentication;

namespace Auth.CoreTests.Tests.Application;

public sealed class RefreshTokenTests
{
    private static readonly DateTime NowUtc = RefreshSessionBuilder.DefaultNowUtc;

    [Fact]
    public async Task RefreshToken_RotatesRefreshSession()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        var publicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        fixture.UserAuthProfiles.Add(ActiveProfile(publicId));
        fixture.RefreshSessions.AddExistingForRefreshToken(
            "refresh-token",
            NowUtc.AddDays(1),
            NowUtc);

        var result = await fixture.Sender.Send(
            new RefreshTokenCommand("refresh-token", DeviceName: null, IpAddress: null));

        var session = result.ShouldSucceed();
        Assert.Equal(publicId, session.UserPublicId);
        Assert.Equal(1, fixture.RefreshSessions.RotateCalls);
        Assert.Equal(2, fixture.RefreshSessions.Sessions.Count);
        Assert.NotEqual("refresh-token", session.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ReplayRevokesActiveSessions()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        var replayedSession = fixture.RefreshSessions.AddExistingForRefreshToken(
            "refresh-token",
            NowUtc.AddDays(1),
            NowUtc);
        replayedSession.Revoke(NowUtc.AddMinutes(-1));
        fixture.RefreshSessions.AddExisting(RefreshSession.Create(
            userId: 10,
            tokenHash: "other-active-hash",
            expiresAtUtc: NowUtc.AddDays(1),
            createdAtUtc: NowUtc,
            deviceName: "Mobile",
            ipAddress: "127.0.0.2"));

        var result = await fixture.Sender.Send(
            new RefreshTokenCommand("refresh-token", DeviceName: null, IpAddress: null));

        result.ShouldFailWith(AuthErrors.RefreshSessionReplayed);
        Assert.Equal(1, fixture.RefreshSessions.RevokeAllCalls);
        Assert.Equal(1, fixture.RefreshSessions.SaveChangesCalls);
        Assert.All(fixture.RefreshSessions.Sessions, session => Assert.NotNull(session.RevokedAtUtc));
        Assert.Equal(0, fixture.RefreshSessions.RotateCalls);
    }

    [Fact]
    public async Task RefreshToken_ExpiredTokenDoesNotRotate()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.RefreshSessions.AddExistingForRefreshToken(
            "refresh-token",
            NowUtc,
            NowUtc.AddDays(-1));

        var result = await fixture.Sender.Send(
            new RefreshTokenCommand("refresh-token", DeviceName: null, IpAddress: null));

        result.ShouldFailWith(AuthErrors.RefreshSessionExpired);
        Assert.Equal(0, fixture.RefreshSessions.RotateCalls);
    }

    [Fact]
    public async Task RefreshToken_InactiveUserDoesNotReceiveNewSession()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.UserAuthProfiles.Add(new UserAuthProfile(
            UserId: 10,
            PublicId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email: "user@example.com",
            Status: "Suspended",
            EmailVerified: true,
            Roles: ["user"]));
        fixture.RefreshSessions.AddExistingForRefreshToken(
            "refresh-token",
            NowUtc.AddDays(1),
            NowUtc);

        var result = await fixture.Sender.Send(
            new RefreshTokenCommand("refresh-token", DeviceName: null, IpAddress: null));

        result.ShouldFailWith(AuthErrors.UserNotAllowed);
        Assert.Equal(0, fixture.RefreshSessions.RotateCalls);
    }

    private static UserAuthProfile ActiveProfile(Guid publicId)
    {
        return new UserAuthProfile(
            UserId: 10,
            PublicId: publicId,
            Email: "user@example.com",
            Status: "Active",
            EmailVerified: true,
            Roles: ["user"]);
    }
}

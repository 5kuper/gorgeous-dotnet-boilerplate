using Auth.Application.Features.Sessions;
using Auth.CoreTests.Support.Fixtures;
using Shared.TestKit.Assertions;

namespace Auth.CoreTests.Tests.Application;

public sealed class LogoutTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Logout_RevokesRefreshSession()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        var session = fixture.RefreshSessions.AddExistingForRefreshToken(
            "refresh-token",
            NowUtc.AddDays(1),
            NowUtc);

        var result = await fixture.Sender.Send(new LogoutCommand("refresh-token"));

        result.ShouldSucceed();
        Assert.NotNull(session.RevokedAtUtc);
        Assert.Equal(1, fixture.RefreshSessions.SaveChangesCalls);
    }

    [Fact]
    public async Task Logout_Succeeds_WhenRefreshSessionDoesNotExist()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);

        var result = await fixture.Sender.Send(new LogoutCommand("missing-token"));

        result.ShouldSucceed();
        Assert.Equal(0, fixture.RefreshSessions.SaveChangesCalls);
    }
}

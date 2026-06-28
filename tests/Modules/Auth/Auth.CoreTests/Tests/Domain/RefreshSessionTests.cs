using Auth.CoreTests.Support.Builders;
using Auth.Domain.Foundation.Errors;
using Shared.TestKit.Assertions;

namespace Auth.CoreTests.Tests.Domain;

public sealed class RefreshSessionTests
{
    [Fact]
    public void ActiveSession_IsRefreshable()
    {
        var session = RefreshSessionBuilder.Active();

        var result = session.EnsureCanRefresh(RefreshSessionBuilder.DefaultNowUtc);

        result.ShouldSucceed();
        Assert.True(session.IsActive(RefreshSessionBuilder.DefaultNowUtc));
    }

    [Fact]
    public void ExpiredSession_FailsWithRefreshSessionExpired()
    {
        var session = RefreshSessionBuilder.Expired();

        var result = session.EnsureCanRefresh(RefreshSessionBuilder.DefaultNowUtc);

        result.ShouldFailWith(AuthErrors.RefreshSessionExpired);
        Assert.False(session.IsActive(RefreshSessionBuilder.DefaultNowUtc));
    }

    [Fact]
    public void RevokedSession_IsTreatedAsReplay()
    {
        var session = RefreshSessionBuilder.Active();
        session.Revoke(RefreshSessionBuilder.DefaultNowUtc.AddMinutes(1));

        var result = session.EnsureCanRefresh(RefreshSessionBuilder.DefaultNowUtc.AddMinutes(2));

        result.ShouldFailWith(AuthErrors.RefreshSessionReplayed);
        Assert.False(session.IsActive(RefreshSessionBuilder.DefaultNowUtc.AddMinutes(2)));
    }

    [Fact]
    public void ReplacedSession_IsTreatedAsReplay()
    {
        var session = RefreshSessionBuilder.Active();
        session.ReplaceBy(newSessionId: 20, RefreshSessionBuilder.DefaultNowUtc.AddMinutes(1));

        var result = session.EnsureCanRefresh(RefreshSessionBuilder.DefaultNowUtc.AddMinutes(2));

        result.ShouldFailWith(AuthErrors.RefreshSessionReplayed);
        Assert.False(session.IsActive(RefreshSessionBuilder.DefaultNowUtc.AddMinutes(2)));
    }

    [Fact]
    public void Revoke_IsIdempotent()
    {
        var session = RefreshSessionBuilder.Active();
        var firstRevokedAtUtc = RefreshSessionBuilder.DefaultNowUtc.AddMinutes(1);
        var secondRevokedAtUtc = RefreshSessionBuilder.DefaultNowUtc.AddMinutes(2);

        session.Revoke(firstRevokedAtUtc);
        session.Revoke(secondRevokedAtUtc);

        Assert.Equal(firstRevokedAtUtc, session.RevokedAtUtc);
    }

    [Fact]
    public void ReplaceBy_StoresReplacementIdAndRevokesSession()
    {
        var session = RefreshSessionBuilder.Active();
        var replacedAtUtc = RefreshSessionBuilder.DefaultNowUtc.AddMinutes(1);

        session.ReplaceBy(newSessionId: 20, replacedAtUtc);

        Assert.Equal(20, session.ReplacedBySessionId);
        Assert.Equal(replacedAtUtc, session.RevokedAtUtc);
        Assert.False(session.IsActive(replacedAtUtc));
    }
}

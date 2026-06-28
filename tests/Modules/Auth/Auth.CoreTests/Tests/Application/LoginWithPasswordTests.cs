using Auth.Application.Features.Sessions;
using Auth.Application.Ports.Identity;
using Auth.CoreTests.Support.Fixtures;
using Auth.Domain.Foundation.Errors;
using Shared.TestKit.Assertions;
using Users.Contracts.Authentication;

namespace Auth.CoreTests.Tests.Application;

public sealed class LoginWithPasswordTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task LoginWithPassword_Fails_WhenCredentialsAreInvalid()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.Identity.SignInResult = new CredentialSignInResult(
            Succeeded: false,
            IsNotAllowed: false,
            UserId: null);

        var result = await fixture.Sender.Send(
            new LoginWithPasswordCommand("user@example.com", "bad", "Browser", "127.0.0.1"));

        result.ShouldFailWith(AuthErrors.InvalidCredentials);
        Assert.Empty(fixture.RefreshSessions.Sessions);
    }

    [Fact]
    public async Task LoginWithPassword_Fails_WhenIdentityReportsNotAllowed()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.Identity.SignInResult = new CredentialSignInResult(
            Succeeded: false,
            IsNotAllowed: true,
            UserId: 10);

        var result = await fixture.Sender.Send(
            new LoginWithPasswordCommand("user@example.com", "Password123!", "Browser", "127.0.0.1"));

        result.ShouldFailWith(AuthErrors.UserNotAllowed);
        Assert.Empty(fixture.RefreshSessions.Sessions);
    }

    [Fact]
    public async Task LoginWithPassword_Fails_WhenEmailIsNotConfirmed()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.UserAuthProfiles.Add(new UserAuthProfile(
            UserId: 10,
            PublicId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email: "user@example.com",
            Status: "Active",
            EmailVerified: false,
            Roles: ["user"]));

        var result = await fixture.Sender.Send(
            new LoginWithPasswordCommand("user@example.com", "Password123!", "Browser", "127.0.0.1"));

        result.ShouldFailWith(AuthErrors.EmailNotConfirmed);
        Assert.Empty(fixture.RefreshSessions.Sessions);
    }

    [Fact]
    public async Task LoginWithPassword_CreatesRefreshSessionAndIssuesAccessToken()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        var publicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        fixture.UserAuthProfiles.Add(new UserAuthProfile(
            UserId: 10,
            PublicId: publicId,
            Email: "user@example.com",
            Status: "Active",
            EmailVerified: true,
            Roles: ["user"]));

        var result = await fixture.Sender.Send(
            new LoginWithPasswordCommand("user@example.com", "Password123!", "Browser", "127.0.0.1"));

        var session = result.ShouldSucceed();
        Assert.Equal("access-token", session.AccessToken);
        Assert.Equal(publicId, session.UserPublicId);
        Assert.NotEmpty(session.RefreshToken);
        Assert.Single(fixture.RefreshSessions.Sessions);
        Assert.Equal(1, fixture.RefreshSessions.SaveChangesCalls);
        Assert.Equal(1, fixture.TokenIssuer.IssueAccessTokenCalls);
    }
}

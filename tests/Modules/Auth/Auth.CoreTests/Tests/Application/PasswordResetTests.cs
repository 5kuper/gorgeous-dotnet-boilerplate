using Auth.Application.Features.PasswordReset;
using Auth.CoreTests.Support.Fixtures;
using Auth.Domain.Foundation.Errors;
using Shared.BuildingBlocks.Core.Results;
using Shared.TestKit.Assertions;

namespace Auth.CoreTests.Tests.Application;

public sealed class PasswordResetTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task RequestPasswordReset_DoesNotSendMessage_WhenEmailDoesNotExist()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.Identity.PasswordResetTokenResult = Result.Success<string?>(null);

        var result = await fixture.Sender.Send(new RequestPasswordResetCommand("missing@example.com"));

        result.ShouldSucceed();
        Assert.Equal(0, fixture.PasswordResetSender.SendCalls);
    }

    [Fact]
    public async Task RequestPasswordReset_SendsMessage_WhenTokenIsGenerated()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);

        var result = await fixture.Sender.Send(new RequestPasswordResetCommand("user@example.com"));

        result.ShouldSucceed();
        Assert.Equal(1, fixture.PasswordResetSender.SendCalls);
    }

    [Fact]
    public async Task ResetPassword_RevokesActiveSessions()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.Identity.ResetPasswordResult = Result.Success(10L);

        var result = await fixture.Sender.Send(
            new ResetPasswordCommand("user@example.com", "reset-token", "NewPassword123!"));

        result.ShouldSucceed();
        Assert.Equal(1, fixture.Identity.ResetPasswordCalls);
        Assert.Equal(1, fixture.RefreshSessions.RevokeAllCalls);
        Assert.Equal(1, fixture.RefreshSessions.SaveChangesCalls);
    }

    [Fact]
    public async Task ResetPassword_ReturnsFailure_WhenIdentityResetFails()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.Identity.ResetPasswordResult = Result.Failure<long>(
            AuthErrors.PasswordResetFailed);

        var result = await fixture.Sender.Send(
            new ResetPasswordCommand("user@example.com", "bad-token", "NewPassword123!"));

        result.ShouldFailWith(AuthErrors.PasswordResetFailed);
        Assert.Equal(0, fixture.RefreshSessions.RevokeAllCalls);
        Assert.Equal(0, fixture.RefreshSessions.SaveChangesCalls);
    }
}

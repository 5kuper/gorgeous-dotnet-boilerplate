using Auth.Application.Features.Verification;
using Auth.Application.Ports.Verification;
using Auth.CoreTests.Support.Fixtures;
using Auth.Domain.Foundation.Errors;
using Gorgeous.Abstractions.Results;
using Shared.TestKit.Assertions;

namespace Auth.CoreTests.Tests.Application;

public sealed class EmailConfirmationTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ConfirmEmail_ConfirmsIdentityAndUsersEmail()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);

        var result = await fixture.Sender.Send(new ConfirmEmailCommand("confirmation-code"));

        result.ShouldSucceed();
        Assert.Equal(1, fixture.Identity.ConfirmEmailCalls);
        Assert.Equal(1, fixture.UsersEmailVerification.ConfirmEmailCalls);
    }

    [Fact]
    public async Task ConfirmEmail_Fails_WhenCodeIsInvalid()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.EmailConfirmationProtector.UnprotectResult =
            Result.Failure<EmailConfirmationPayload>(
                AuthErrors.InvalidEmailConfirmationCode);

        var result = await fixture.Sender.Send(new ConfirmEmailCommand("bad-code"));

        result.ShouldFailWith(AuthErrors.InvalidEmailConfirmationCode);
        Assert.Equal(0, fixture.Identity.ConfirmEmailCalls);
        Assert.Equal(0, fixture.UsersEmailVerification.ConfirmEmailCalls);
    }
}

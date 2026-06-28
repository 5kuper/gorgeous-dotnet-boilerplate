using Auth.Application.Features.Registration;
using Auth.CoreTests.Support.Fixtures;
using Auth.Domain.Foundation.Errors;
using Shared.BuildingBlocks.Core.Results;
using Shared.TestKit.Assertions;

namespace Auth.CoreTests.Tests.Application;

public sealed class RegisterWithEmailPasswordTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task RegisterWithEmailPassword_SendsConfirmationAfterSuccessfulRegistration()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);

        var result = await fixture.Sender.Send(
            new RegisterWithEmailPasswordCommand(
                "user@example.com",
                "Password123!",
                "Test User"));

        var registration = result.ShouldSucceed();
        Assert.True(registration.EmailConfirmationRequired);
        Assert.Equal("user@example.com", fixture.UsersRegistration.LastRequest?.Email);
        Assert.Equal("EmailPassword", fixture.UsersRegistration.LastRequest?.RegistrationMethod);
        Assert.True(fixture.UsersRegistration.LastRequest?.RequireEmailConfirmation);
        Assert.Equal(1, fixture.RegistrationTransaction.ExecuteCalls);
        Assert.Equal(1, fixture.Identity.CreateUserCalls);
        Assert.Equal(1, fixture.Identity.GenerateEmailConfirmationTokenCalls);
        Assert.Equal(1, fixture.EmailConfirmationSender.SendCalls);
        Assert.Equal("user@example.com", fixture.EmailConfirmationSender.LastEmail);
        Assert.Equal("confirmation-code", fixture.EmailConfirmationSender.LastCode);
    }

    [Fact]
    public async Task RegisterWithEmailPassword_DoesNotSendConfirmation_WhenIdentityCreationFails()
    {
        using var fixture = new AuthApplicationFixture(NowUtc);
        fixture.Identity.CreateUserResult = Result.Failure(
            AuthErrors.IdentityOperationFailed);

        var result = await fixture.Sender.Send(
            new RegisterWithEmailPasswordCommand(
                "user@example.com",
                "Password123!",
                "Test User"));

        result.ShouldFailWith(AuthErrors.IdentityOperationFailed);
        Assert.Equal(1, fixture.Identity.CreateUserCalls);
        Assert.Equal(0, fixture.Identity.GenerateEmailConfirmationTokenCalls);
        Assert.Equal(0, fixture.EmailConfirmationSender.SendCalls);
    }
}

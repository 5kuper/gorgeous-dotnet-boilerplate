using Shared.TestKit.Assertions;
using Users.Domain.Entities;
using Users.Domain.Foundation.Errors;
using Users.Domain.Foundation.Primitives;

namespace Users.CoreTests.Tests.Domain;

public sealed class UserRegistrationTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Register_TrimsDisplayName()
    {
        var result = User.Register(
            "user@example.com",
            "  Test User  ",
            RegistrationMethod.EmailPassword,
            requireEmailConfirmation: false,
            NowUtc);

        var user = result.ShouldSucceed();

        Assert.Equal("Test User", user.DisplayName);
    }

    [Fact]
    public void Register_NormalizesEmail()
    {
        var result = User.Register(
            "  user@example.com  ",
            "Test User",
            RegistrationMethod.EmailPassword,
            requireEmailConfirmation: false,
            NowUtc);

        var user = result.ShouldSucceed();

        Assert.Equal("USER@EXAMPLE.COM", user.Email);
    }

    [Fact]
    public void Register_Fails_WhenDisplayNameIsBlank()
    {
        var result = User.Register(
            "user@example.com",
            " ",
            RegistrationMethod.EmailPassword,
            requireEmailConfirmation: false,
            NowUtc);

        result.ShouldFailWith(UserErrors.DisplayNameRequired);
    }

    [Fact]
    public void Register_Fails_WhenEmailIsWhitespace()
    {
        var result = User.Register(
            " ",
            "Test User",
            RegistrationMethod.EmailPassword,
            requireEmailConfirmation: false,
            NowUtc);

        result.ShouldFailWith(UserErrors.InvalidEmail);
    }

    [Fact]
    public void Register_CreatesActiveUser_WhenConfirmationIsNotRequired()
    {
        var result = User.Register(
            "user@example.com",
            "Test User",
            RegistrationMethod.EmailPassword,
            requireEmailConfirmation: false,
            NowUtc);

        var user = result.ShouldSucceed();

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.True(user.EmailVerified);
        Assert.True(user.CanAuthenticate());
    }

    [Fact]
    public void Register_CreatesPendingActivationUser_WhenEmailPasswordConfirmationIsRequired()
    {
        var result = User.Register(
            "user@example.com",
            "Test User",
            RegistrationMethod.EmailPassword,
            requireEmailConfirmation: true,
            NowUtc);

        var user = result.ShouldSucceed();

        Assert.Equal(UserStatus.PendingActivation, user.Status);
        Assert.False(user.EmailVerified);
        Assert.False(user.CanAuthenticate());
    }
}

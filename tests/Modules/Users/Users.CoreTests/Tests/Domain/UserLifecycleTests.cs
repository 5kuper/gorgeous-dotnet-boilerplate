using Shared.TestKit.Assertions;
using Users.CoreTests.Support.Builders;
using Users.Domain.Foundation.Primitives;

namespace Users.CoreTests.Tests.Domain;

public sealed class UserLifecycleTests
{
    [Fact]
    public void ConfirmEmail_MarksEmailVerified()
    {
        var user = UserBuilder.PendingEmailPasswordUser();
        var confirmedAtUtc = UserBuilder.DefaultNowUtc.AddMinutes(5);

        Assert.False(user.EmailVerified);

        var result = user.ConfirmEmail(confirmedAtUtc);

        result.ShouldSucceed();
        Assert.True(user.EmailVerified);
        Assert.Equal(confirmedAtUtc, user.UpdatedAtUtc);
    }

    [Fact]
    public void ConfirmEmail_ActivatesPendingEmailPasswordUser()
    {
        var user = UserBuilder.PendingEmailPasswordUser();

        var result = user.ConfirmEmail(UserBuilder.DefaultNowUtc.AddMinutes(5));

        result.ShouldSucceed();
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.True(user.CanAuthenticate());
    }

    [Fact]
    public void Suspend_SetsSuspendedStatus()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();
        var suspendedAtUtc = UserBuilder.DefaultNowUtc.AddMinutes(5);

        var result = user.Suspend(suspendedAtUtc);

        result.ShouldSucceed();
        Assert.Equal(UserStatus.Suspended, user.Status);
        Assert.False(user.CanAuthenticate());
        Assert.Equal(suspendedAtUtc, user.UpdatedAtUtc);
    }
}

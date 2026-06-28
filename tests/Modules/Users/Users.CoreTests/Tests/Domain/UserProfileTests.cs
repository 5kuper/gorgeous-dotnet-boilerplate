using Shared.TestKit.Assertions;
using Users.CoreTests.Support.Builders;
using Users.Domain.Foundation.Errors;

namespace Users.CoreTests.Tests.Domain;

public sealed class UserProfileTests
{
    [Fact]
    public void UpdateProfile_UpdatesTrimmedDisplayName()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();
        var updatedAtUtc = UserBuilder.DefaultNowUtc.AddMinutes(5);

        var result = user.UpdateProfile("  Updated User  ", updatedAtUtc);

        result.ShouldSucceed();
        Assert.Equal("Updated User", user.DisplayName);
        Assert.Equal(updatedAtUtc, user.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateProfile_Fails_WhenDisplayNameIsBlank()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();

        var result = user.UpdateProfile(" ", UserBuilder.DefaultNowUtc.AddMinutes(5));

        result.ShouldFailWith(UserErrors.DisplayNameRequired);
    }
}

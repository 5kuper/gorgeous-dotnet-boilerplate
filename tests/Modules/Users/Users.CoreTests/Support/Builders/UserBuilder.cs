using Users.Domain.Entities;
using Users.Domain.Foundation.Primitives;

namespace Users.CoreTests.Support.Builders;

internal static class UserBuilder
{
    public static readonly DateTime DefaultNowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    public static User ActiveEmailPasswordUser()
    {
        return User.Register(
                "user@example.com",
                "Test User",
                RegistrationMethod.EmailPassword,
                requireEmailConfirmation: false,
                DefaultNowUtc)
            .Value;
    }

    public static User PendingEmailPasswordUser()
    {
        return User.Register(
                "user@example.com",
                "Test User",
                RegistrationMethod.EmailPassword,
                requireEmailConfirmation: true,
                DefaultNowUtc)
            .Value;
    }
}

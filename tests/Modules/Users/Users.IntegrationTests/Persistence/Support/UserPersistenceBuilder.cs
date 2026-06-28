using Users.Domain.Entities;
using Users.Domain.Foundation.Primitives;

namespace Users.IntegrationTests.Persistence.Support;

internal static class UserPersistenceBuilder
{
    public static User CreateEmailPasswordUser(
        DateTime nowUtc,
        string email = "user@example.com",
        string name = "Test User")
    {
        return User.Register(
                email,
                name,
                RegistrationMethod.EmailPassword,
                requireEmailConfirmation: false,
                nowUtc)
            .Value;
    }
}

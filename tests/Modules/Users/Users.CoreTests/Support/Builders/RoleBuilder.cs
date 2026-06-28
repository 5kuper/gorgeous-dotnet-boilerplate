using Shared.TestKit.TestData;
using Users.Domain.Entities;

namespace Users.CoreTests.Support.Builders;

internal static class RoleBuilder
{
    public static Role UserRole(long id = 1)
    {
        return Role.Create(
                "user",
                "User",
                "Default user",
                isSystem: true,
                UserBuilder.DefaultNowUtc)
            .Value
            .SetId(id);
    }

    public static Role AdminRole(long id = 2)
    {
        return Role.Create(
                "admin",
                "Administrator",
                "System administrator",
                isSystem: true,
                UserBuilder.DefaultNowUtc)
            .Value
            .SetId(id);
    }
}

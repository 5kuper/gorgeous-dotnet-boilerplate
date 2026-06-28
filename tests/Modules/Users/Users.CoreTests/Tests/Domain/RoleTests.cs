using Shared.TestKit.Assertions;
using Users.Domain.Entities;
using Users.Domain.Foundation.Errors;

namespace Users.CoreTests.Tests.Domain;

public sealed class RoleTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesCodeAndTrimsNameAndDescription()
    {
        var result = Role.Create(
            "  Admin  ",
            "  Administrator  ",
            "  System administrator  ",
            isSystem: true,
            NowUtc);

        var role = result.ShouldSucceed();

        Assert.Equal("admin", role.Code);
        Assert.Equal("Administrator", role.Name);
        Assert.Equal("System administrator", role.Description);
        Assert.True(role.IsSystem);
    }

    [Fact]
    public void Create_Fails_WhenCodeIsBlank()
    {
        var result = Role.Create(" ", "Administrator", null, isSystem: true, NowUtc);

        result.ShouldFailWith(RoleErrors.CodeRequired);
    }

    [Fact]
    public void Create_Fails_WhenNameIsBlank()
    {
        var result = Role.Create("admin", " ", null, isSystem: true, NowUtc);

        result.ShouldFailWith(RoleErrors.NameRequired);
    }

    [Fact]
    public void Update_TrimsNameAndDescription()
    {
        var role = Role.Create("admin", "Administrator", null, isSystem: true, NowUtc).Value;
        var updatedAtUtc = NowUtc.AddMinutes(5);

        role.Update("  Support Lead  ", "  Helps users  ", updatedAtUtc);

        Assert.Equal("Support Lead", role.Name);
        Assert.Equal("Helps users", role.Description);
        Assert.Equal(updatedAtUtc, role.UpdatedAtUtc);
    }
}

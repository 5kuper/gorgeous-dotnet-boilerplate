using Shared.TestKit.Assertions;
using Users.CoreTests.Support.Builders;
using Users.Domain.Foundation.Errors;

namespace Users.CoreTests.Tests.Domain;

public sealed class UserRolesTests
{
    [Fact]
    public void AssignRole_AssignsRole()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();
        var assignedAtUtc = UserBuilder.DefaultNowUtc.AddMinutes(5);

        var result = user.AssignRole(roleId: 10, assignedAtUtc);

        result.ShouldSucceed();
        var role = Assert.Single(user.Roles);
        Assert.Equal(10, role.RoleId);
        Assert.Equal(assignedAtUtc, role.AssignedAtUtc);
        Assert.Null(role.AssignedByUserId);
    }

    [Fact]
    public void AssignRole_RecordsAssignedByUser_WhenSupplied()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();

        var result = user.AssignRole(
            roleId: 10,
            UserBuilder.DefaultNowUtc.AddMinutes(5),
            assignedByUserId: 99);

        result.ShouldSucceed();
        var role = Assert.Single(user.Roles);
        Assert.Equal(99, role.AssignedByUserId);
    }

    [Fact]
    public void AssignRole_Fails_WhenRoleIsAlreadyAssigned()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();
        user.AssignRole(roleId: 10, UserBuilder.DefaultNowUtc.AddMinutes(5)).ShouldSucceed();

        var result = user.AssignRole(roleId: 10, UserBuilder.DefaultNowUtc.AddMinutes(6));

        result.ShouldFailWith(UserErrors.RoleAlreadyAssigned);
        Assert.Single(user.Roles);
    }

    [Fact]
    public void RevokeRole_RemovesAssignedRole()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();
        user.AssignRole(roleId: 10, UserBuilder.DefaultNowUtc.AddMinutes(5)).ShouldSucceed();

        var result = user.RevokeRole(roleId: 10, UserBuilder.DefaultNowUtc.AddMinutes(6));

        result.ShouldSucceed();
        Assert.Empty(user.Roles);
    }

    [Fact]
    public void RevokeRole_Fails_WhenRoleIsNotAssigned()
    {
        var user = UserBuilder.ActiveEmailPasswordUser();

        var result = user.RevokeRole(roleId: 10, UserBuilder.DefaultNowUtc.AddMinutes(5));

        result.ShouldFailWith(UserErrors.RoleNotAssigned);
    }
}

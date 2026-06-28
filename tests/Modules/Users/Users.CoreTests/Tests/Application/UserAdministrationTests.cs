using Shared.TestKit.Assertions;
using Users.Application.Features.Administration;
using Users.Application.Features.Profile;
using Users.Application.Features.Roles;
using Users.CoreTests.Support.Builders;
using Users.CoreTests.Support.Fixtures;

namespace Users.CoreTests.Tests.Application;

public sealed class UserAdministrationTests
{
    [Fact]
    public async Task UpdateProfile_UpdatesUser()
    {
        using var fixture = new UsersApplicationFixture(UserBuilder.DefaultNowUtc);
        var user = UserBuilder.ActiveEmailPasswordUser();
        fixture.UserRepository.AddExisting(user);

        var result = await fixture.Sender.Send(
            new UpdateProfileCommand(user.PublicId, "  Updated User  "));

        result.ShouldSucceed();
        Assert.Equal("Updated User", user.DisplayName);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task AssignRole_AssignsRoleWithActor()
    {
        using var fixture = new UsersApplicationFixture(UserBuilder.DefaultNowUtc);
        var user = UserBuilder.ActiveEmailPasswordUser();
        var actor = UserBuilder.ActiveEmailPasswordUser();
        fixture.UserRepository.AddExisting(user);
        fixture.UserRepository.AddExisting(actor);
        fixture.RoleRepository.AddExisting(RoleBuilder.AdminRole(id: 20));

        var result = await fixture.Sender.Send(
            new AssignRoleCommand(user.PublicId, " ADMIN ", actor.PublicId));

        result.ShouldSucceed();
        var role = Assert.Single(user.Roles);
        Assert.Equal(20, role.RoleId);
        Assert.Equal(actor.Id, role.AssignedByUserId);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task RevokeRole_RemovesRole()
    {
        using var fixture = new UsersApplicationFixture(UserBuilder.DefaultNowUtc);
        var user = UserBuilder.ActiveEmailPasswordUser();
        user.AssignRole(roleId: 20, UserBuilder.DefaultNowUtc).ShouldSucceed();
        fixture.UserRepository.AddExisting(user);
        fixture.RoleRepository.AddExisting(RoleBuilder.AdminRole(id: 20));

        var result = await fixture.Sender.Send(
            new RevokeRoleCommand(user.PublicId, "admin"));

        result.ShouldSucceed();
        Assert.Empty(user.Roles);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task SuspendUser_SuspendsUser()
    {
        using var fixture = new UsersApplicationFixture(UserBuilder.DefaultNowUtc);
        var user = UserBuilder.ActiveEmailPasswordUser();
        fixture.UserRepository.AddExisting(user);

        var result = await fixture.Sender.Send(new SuspendUserCommand(user.PublicId));

        result.ShouldSucceed();
        Assert.False(user.CanAuthenticate());
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCalls);
    }
}

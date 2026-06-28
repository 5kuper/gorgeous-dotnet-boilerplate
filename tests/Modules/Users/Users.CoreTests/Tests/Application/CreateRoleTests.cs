using Shared.TestKit.Assertions;
using Users.Application.Features.Roles;
using Users.CoreTests.Support.Builders;
using Users.CoreTests.Support.Fixtures;
using Users.Domain.Foundation.Errors;

namespace Users.CoreTests.Tests.Application;

public sealed class CreateRoleTests
{
    [Fact]
    public async Task CreateRole_AddsRole()
    {
        using var fixture = new UsersApplicationFixture(UserBuilder.DefaultNowUtc);

        var result = await fixture.Sender.Send(
            new CreateRoleCommand(" Support ", " Support ", " Helps users ", IsSystem: false));

        result.ShouldSucceed();
        var role = Assert.Single(fixture.RoleRepository.Roles);
        Assert.Equal("support", role.Code);
        Assert.Equal("Support", role.Name);
        Assert.Equal("Helps users", role.Description);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateRole_Fails_WhenCodeAlreadyExists()
    {
        using var fixture = new UsersApplicationFixture(UserBuilder.DefaultNowUtc);
        fixture.RoleRepository.AddExisting(RoleBuilder.AdminRole());

        var result = await fixture.Sender.Send(
            new CreateRoleCommand(" ADMIN ", "Administrator", null, IsSystem: true));

        result.ShouldFailWith(RoleErrors.CodeAlreadyUsed);
        Assert.Single(fixture.RoleRepository.Roles);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCalls);
    }
}

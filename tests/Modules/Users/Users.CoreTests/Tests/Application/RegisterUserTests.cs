using Shared.TestKit.Assertions;
using Users.Application.Features.Registration;
using Users.CoreTests.Support.Builders;
using Users.CoreTests.Support.Fixtures;
using Users.Domain.Entities;
using Users.Domain.Foundation.Errors;

namespace Users.CoreTests.Tests.Application;

public sealed class RegisterUserTests
{
    private static readonly DateTime NowUtc = UserBuilder.DefaultNowUtc;

    [Fact]
    public async Task RegisterUser_AssignsDefaultUserRole()
    {
        using var fixture = new UsersApplicationFixture(NowUtc);
        fixture.RoleRepository.AddExisting(RoleBuilder.UserRole(id: 10));

        var result = await fixture.Sender.Send(
            new RegisterUserCommand(
                "user@example.com",
                "Test User",
                "EmailPassword",
                RequireEmailConfirmation: true));

        var registeredUser = result.ShouldSucceed();
        var user = Assert.Single(fixture.UserRepository.Users);
        var role = Assert.Single(user.Roles);
        Assert.Equal(registeredUser.UserId, user.Id);
        Assert.Equal(10, role.RoleId);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterUser_Fails_WhenEmailAlreadyExists()
    {
        using var fixture = new UsersApplicationFixture(NowUtc);
        fixture.RoleRepository.AddExisting(RoleBuilder.UserRole(id: 10));
        fixture.UserRepository.AddExisting(User.Register(
                "user@example.com",
                "Existing User",
                Users.Domain.Foundation.Primitives.RegistrationMethod.EmailPassword,
                requireEmailConfirmation: false,
                NowUtc)
            .Value);

        var result = await fixture.Sender.Send(
            new RegisterUserCommand(
                " USER@example.com ",
                "Test User",
                "EmailPassword",
                RequireEmailConfirmation: true));

        result.ShouldFailWith(UserErrors.EmailAlreadyUsed);
        Assert.Single(fixture.UserRepository.Users);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterUser_Fails_WhenDefaultRoleIsMissing()
    {
        using var fixture = new UsersApplicationFixture(NowUtc);

        var result = await fixture.Sender.Send(
            new RegisterUserCommand(
                "user@example.com",
                "Test User",
                "EmailPassword",
                RequireEmailConfirmation: true));

        result.ShouldFailWith(RoleErrors.NotFound);
        Assert.Empty(fixture.UserRepository.Users);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCalls);
    }
}

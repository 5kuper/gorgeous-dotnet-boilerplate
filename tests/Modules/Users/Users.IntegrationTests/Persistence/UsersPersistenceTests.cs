using static Users.IntegrationTests.Persistence.Support.UserPersistenceBuilder;
using static Users.IntegrationTests.Persistence.Support.UsersPersistenceServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.IntegrationTesting.Database;
using Users.Contracts.Authentication;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence;

namespace Users.IntegrationTests.Persistence;

public sealed class UsersPersistenceTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task UsersDbContext_SeedsDefaultRoles()
    {
        await using var database = new SqliteTestDatabase<UsersDbContext>();
        await database.InitializeAsync(options => new UsersDbContext(options));

        await using var context = database.CreateContext(options => new UsersDbContext(options));
        string[] roleCodes = await context.Roles
            .OrderBy(role => role.Id)
            .Select(role => role.Code)
            .ToArrayAsync();

        Assert.Equal(["user", "admin", "support"], roleCodes);
    }

    [Fact]
    public async Task UsersDbContext_PersistsUserRoleAssignment()
    {
        await using var database = new SqliteTestDatabase<UsersDbContext>();
        await database.InitializeAsync(options => new UsersDbContext(options));

        await using (var context = database.CreateContext(options => new UsersDbContext(options)))
        {
            var user = CreateEmailPasswordUser(NowUtc);
            user.AssignRole(roleId: 1, NowUtc, assignedByUserId: null);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        await using (var context = database.CreateContext(options => new UsersDbContext(options)))
        {
            var user = await context.Users
                .Include(storedUser => storedUser.Roles)
                .SingleAsync();

            var role = Assert.Single(user.Roles);
            Assert.Equal("USER@EXAMPLE.COM", user.Email);
            Assert.Equal(1, role.RoleId);
        }
    }

    [Fact]
    public async Task UserAuthProfileReader_ReturnsRolesAndStatus()
    {
        await using var database = new SqliteTestDatabase<UsersDbContext>();
        await database.InitializeAsync(options => new UsersDbContext(options));

        Guid publicId;
        long userId;
        await using (var context = database.CreateContext(options => new UsersDbContext(options)))
        {
            var user = CreateEmailPasswordUser(NowUtc);
            user.AssignRole(roleId: 1, NowUtc, assignedByUserId: null);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            publicId = user.PublicId;
            userId = user.Id;
        }

        await using var readContext = database.CreateContext(options => new UsersDbContext(options));
        using var serviceProvider = CreateServiceProvider(readContext);
        var reader = serviceProvider.GetRequiredService<IUserAuthProfileReader>();

        var profile = await reader.GetByUserIdAsync(userId);

        Assert.NotNull(profile);
        Assert.Equal(publicId, profile.PublicId);
        Assert.Equal("Active", profile.Status);
        Assert.True(profile.EmailVerified);
        Assert.Equal(["user"], profile.Roles);
    }

    [Fact]
    public async Task UsersDbContext_EnforcesUniqueRoleCode()
    {
        await using var database = new SqliteTestDatabase<UsersDbContext>();
        await database.InitializeAsync(options => new UsersDbContext(options));

        await using var context = database.CreateContext(options => new UsersDbContext(options));
        await context.Roles.AddAsync(Role.Create("custom", "Custom", null, isSystem: false, NowUtc).Value);
        await context.Roles.AddAsync(Role.Create(" custom ", "Duplicate", null, isSystem: false, NowUtc).Value);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}

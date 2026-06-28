using static Auth.IntegrationTests.Persistence.Support.AuthPersistenceServices;
using static Auth.IntegrationTests.Persistence.Support.RefreshSessionPersistenceBuilder;

using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.IntegrationTesting.Database;

namespace Auth.IntegrationTests.Persistence;

public sealed class AuthPersistenceTests
{
    private static readonly DateTime NowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task AuthDbContext_PersistsRefreshSession()
    {
        await using var database = new SqliteTestDatabase<AuthDbContext>();
        await database.InitializeAsync(options => new AuthDbContext(options));

        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            await context.RefreshSessions.AddAsync(RefreshSession.Create(
                userId: 10,
                tokenHash: "token-hash",
                expiresAtUtc: NowUtc.AddDays(1),
                createdAtUtc: NowUtc,
                deviceName: "Browser",
                ipAddress: "127.0.0.1"));
            await context.SaveChangesAsync();
        }

        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            var session = await context.RefreshSessions.SingleAsync();

            Assert.Equal(10, session.UserId);
            Assert.Equal("token-hash", session.TokenHash);
            Assert.True(session.IsActive(NowUtc));
        }
    }

    [Fact]
    public async Task AuthDbContext_EnforcesUniqueRefreshTokenHash()
    {
        await using var database = new SqliteTestDatabase<AuthDbContext>();
        await database.InitializeAsync(options => new AuthDbContext(options));

        await using var context = database.CreateContext(options => new AuthDbContext(options));
        await context.RefreshSessions.AddAsync(CreateSession("duplicate-hash", NowUtc));
        await context.RefreshSessions.AddAsync(CreateSession("duplicate-hash", NowUtc));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task RefreshSessionRepository_RotatesRefreshSession()
    {
        await using var database = new SqliteTestDatabase<AuthDbContext>();
        await database.InitializeAsync(options => new AuthDbContext(options));

        long currentSessionId;
        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            var currentSession = CreateSession("current-hash", NowUtc);
            await context.RefreshSessions.AddAsync(currentSession);
            await context.SaveChangesAsync();
            currentSessionId = currentSession.Id;
        }

        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            using var serviceProvider = CreateServiceProvider(context);
            var repository = serviceProvider.GetRequiredService<IRefreshSessionRepository>();
            var currentSession = await repository.GetByTokenHashAsync("current-hash");

            Assert.NotNull(currentSession);
            await repository.RotateAsync(
                currentSession,
                CreateSession("new-hash", NowUtc),
                NowUtc.AddMinutes(5));
        }

        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            var currentSession = await context.RefreshSessions.FindAsync(currentSessionId);
            var newSession = await context.RefreshSessions.SingleAsync(session => session.TokenHash == "new-hash");

            Assert.NotNull(currentSession);
            Assert.Equal(newSession.Id, currentSession.ReplacedBySessionId);
            Assert.NotNull(currentSession.RevokedAtUtc);
        }
    }

    [Fact]
    public async Task RefreshSessionRepository_ConcurrentRotateFailsWithConcurrencyException()
    {
        await using var database = new SqliteTestDatabase<AuthDbContext>();
        await database.InitializeAsync(options => new AuthDbContext(options));

        await using (var seedContext = database.CreateContext(options => new AuthDbContext(options)))
        {
            await seedContext.RefreshSessions.AddAsync(CreateSession("current-hash", NowUtc));
            await seedContext.SaveChangesAsync();
        }

        await using var firstContext = database.CreateContext(options => new AuthDbContext(options));
        await using var secondContext = database.CreateContext(options => new AuthDbContext(options));
        using var firstServiceProvider = CreateServiceProvider(firstContext);
        using var secondServiceProvider = CreateServiceProvider(secondContext);
        var firstRepository = firstServiceProvider.GetRequiredService<IRefreshSessionRepository>();
        var secondRepository = secondServiceProvider.GetRequiredService<IRefreshSessionRepository>();

        var firstCurrentSession = await firstRepository.GetByTokenHashAsync("current-hash");
        var secondCurrentSession = await secondRepository.GetByTokenHashAsync("current-hash");

        Assert.NotNull(firstCurrentSession);
        Assert.NotNull(secondCurrentSession);

        await firstRepository.RotateAsync(
            firstCurrentSession,
            CreateSession("first-new-hash", NowUtc),
            NowUtc.AddMinutes(5));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            secondRepository.RotateAsync(
                secondCurrentSession,
                CreateSession("second-new-hash", NowUtc),
                NowUtc.AddMinutes(6)));

        await using var verifyContext = database.CreateContext(options => new AuthDbContext(options));
        Assert.True(await verifyContext.RefreshSessions.AnyAsync(session => session.TokenHash == "first-new-hash"));
        Assert.False(await verifyContext.RefreshSessions.AnyAsync(session => session.TokenHash == "second-new-hash"));
    }

    [Fact]
    public async Task RefreshSessionRepository_RevokesActiveSessionsForUser()
    {
        await using var database = new SqliteTestDatabase<AuthDbContext>();
        await database.InitializeAsync(options => new AuthDbContext(options));

        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            await context.RefreshSessions.AddAsync(CreateSession("active-1", NowUtc));
            await context.RefreshSessions.AddAsync(CreateSession("active-2", NowUtc));
            await context.RefreshSessions.AddAsync(RefreshSession.Create(
                userId: 11,
                tokenHash: "other-user",
                expiresAtUtc: NowUtc.AddDays(1),
                createdAtUtc: NowUtc,
                deviceName: null,
                ipAddress: null));
            await context.SaveChangesAsync();
        }

        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            using var serviceProvider = CreateServiceProvider(context);
            var repository = serviceProvider.GetRequiredService<IRefreshSessionRepository>();

            await repository.RevokeAllActiveSessionsForUserAsync(userId: 10, NowUtc.AddMinutes(5));
            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext(options => new AuthDbContext(options)))
        {
            var userSessions = await context.RefreshSessions
                .Where(session => session.UserId == 10)
                .ToArrayAsync();
            var otherUserSession = await context.RefreshSessions.SingleAsync(session => session.UserId == 11);

            Assert.All(userSessions, session => Assert.NotNull(session.RevokedAtUtc));
            Assert.Null(otherUserSession.RevokedAtUtc);
        }
    }
}

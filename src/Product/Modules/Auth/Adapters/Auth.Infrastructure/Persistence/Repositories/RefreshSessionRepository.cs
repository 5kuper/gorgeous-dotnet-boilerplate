using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence.Repositories;

internal sealed class RefreshSessionRepository(AuthDbContext dbContext) : IRefreshSessionRepository
{
    public Task<RefreshSession?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default)
    {
        return dbContext.RefreshSessions
            .FirstOrDefaultAsync(session => session.TokenHash == tokenHash, ct);
    }

    public async Task<IReadOnlyCollection<RefreshSession>> GetActiveSessionsByUserIdAsync(
        long userId,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        return await dbContext.RefreshSessions
            .Where(session =>
                session.UserId == userId
                && session.RevokedAtUtc == null
                && session.ReplacedBySessionId == null
                && session.ExpiresAtUtc > nowUtc)
            .ToArrayAsync(ct);
    }

    public async Task AddAsync(RefreshSession session, CancellationToken ct = default)
    {
        await dbContext.RefreshSessions.AddAsync(session, ct);
    }

    public async Task RotateAsync(
        RefreshSession currentSession,
        RefreshSession newSession,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        await dbContext.RefreshSessions.AddAsync(newSession, ct);
        await dbContext.SaveChangesAsync(ct);

        currentSession.ReplaceBy(newSession.Id, nowUtc);
        await dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
    }

    public async Task RevokeAllActiveSessionsForUserAsync(
        long userId,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        var sessions = await GetActiveSessionsByUserIdAsync(
            userId,
            nowUtc,
            ct);

        foreach (var session in sessions)
        {
            session.Revoke(nowUtc);
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}


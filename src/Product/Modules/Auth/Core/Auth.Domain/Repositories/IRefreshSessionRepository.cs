using Auth.Domain.Entities;

namespace Auth.Domain.Repositories;

public interface IRefreshSessionRepository
{
    Task<RefreshSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    Task<IReadOnlyCollection<RefreshSession>> GetActiveSessionsByUserIdAsync(
        long userId,
        DateTime nowUtc,
        CancellationToken ct = default);

    Task AddAsync(RefreshSession session, CancellationToken ct = default);

    Task RotateAsync(
        RefreshSession currentSession,
        RefreshSession newSession,
        DateTime nowUtc,
        CancellationToken ct = default);

    Task RevokeAllActiveSessionsForUserAsync(
        long userId,
        DateTime nowUtc,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}


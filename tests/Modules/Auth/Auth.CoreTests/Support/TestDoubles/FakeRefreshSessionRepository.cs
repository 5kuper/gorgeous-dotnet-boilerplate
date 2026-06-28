using System.Security.Cryptography;
using System.Text;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Shared.TestKit.TestData;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeRefreshSessionRepository : IRefreshSessionRepository
{
    private long _nextId = 1;
    private readonly List<RefreshSession> _sessions = [];

    public IReadOnlyCollection<RefreshSession> Sessions => _sessions;

    public int RotateCalls { get; private set; }

    public int RevokeAllCalls { get; private set; }

    public int SaveChangesCalls { get; private set; }

    public Task<RefreshSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return Task.FromResult(_sessions.FirstOrDefault(session => session.TokenHash == tokenHash));
    }

    public Task<IReadOnlyCollection<RefreshSession>> GetActiveSessionsByUserIdAsync(
        long userId,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        IReadOnlyCollection<RefreshSession> sessions = _sessions
            .Where(session => session.UserId == userId && session.IsActive(nowUtc))
            .ToArray();

        return Task.FromResult(sessions);
    }

    public Task AddAsync(RefreshSession session, CancellationToken ct = default)
    {
        AddExisting(session);

        return Task.CompletedTask;
    }

    public Task RotateAsync(
        RefreshSession currentSession,
        RefreshSession newSession,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        RotateCalls++;
        AddExisting(newSession);
        currentSession.ReplaceBy(newSession.Id, nowUtc);

        return Task.CompletedTask;
    }

    public async Task RevokeAllActiveSessionsForUserAsync(
        long userId,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        RevokeAllCalls++;
        var sessions = await GetActiveSessionsByUserIdAsync(userId, nowUtc, ct);

        foreach (var session in sessions)
        {
            session.Revoke(nowUtc);
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalls++;

        return Task.CompletedTask;
    }

    public void AddExisting(RefreshSession session)
    {
        if (session.Id == 0)
        {
            session.SetId(_nextId++);
        }

        _sessions.Add(session);
    }

    public RefreshSession AddExistingForRefreshToken(
        string refreshToken,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        long userId = 10,
        string? deviceName = "Browser",
        string? ipAddress = "127.0.0.1")
    {
        var session = RefreshSession.Create(
            userId,
            Sha256(refreshToken),
            expiresAtUtc,
            createdAtUtc,
            deviceName,
            ipAddress);

        AddExisting(session);

        return session;
    }

    private static string Sha256(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }
}

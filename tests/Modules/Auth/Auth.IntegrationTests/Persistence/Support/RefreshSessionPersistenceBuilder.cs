using Auth.Domain.Entities;

namespace Auth.IntegrationTests.Persistence.Support;

internal static class RefreshSessionPersistenceBuilder
{
    public static RefreshSession CreateSession(string tokenHash, DateTime nowUtc, long userId = 10)
    {
        return RefreshSession.Create(
            userId,
            tokenHash,
            expiresAtUtc: nowUtc.AddDays(1),
            createdAtUtc: nowUtc,
            deviceName: "Browser",
            ipAddress: "127.0.0.1");
    }
}

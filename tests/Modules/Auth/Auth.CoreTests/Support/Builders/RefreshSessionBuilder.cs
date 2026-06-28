using Auth.Domain.Entities;

namespace Auth.CoreTests.Support.Builders;

internal static class RefreshSessionBuilder
{
    public static readonly DateTime DefaultNowUtc = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);

    public static RefreshSession Active()
    {
        return RefreshSession.Create(
            userId: 10,
            tokenHash: "token-hash",
            expiresAtUtc: DefaultNowUtc.AddDays(1),
            createdAtUtc: DefaultNowUtc,
            deviceName: "Browser",
            ipAddress: "127.0.0.1");
    }

    public static RefreshSession Expired()
    {
        return RefreshSession.Create(
            userId: 10,
            tokenHash: "token-hash",
            expiresAtUtc: DefaultNowUtc,
            createdAtUtc: DefaultNowUtc.AddDays(-1),
            deviceName: "Browser",
            ipAddress: "127.0.0.1");
    }
}

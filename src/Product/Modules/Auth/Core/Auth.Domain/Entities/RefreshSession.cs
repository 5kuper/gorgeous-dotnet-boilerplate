using Auth.Domain.Foundation.Errors;
using Shared.Kernel.BuildingBlocks;
using Gorgeous.Abstractions.Results;

namespace Auth.Domain.Entities;

public sealed class RefreshSession : AggregateRoot<long>
{
    private RefreshSession()
    {
    }

    private RefreshSession(
        long userId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        string? deviceName,
        string? ipAddress)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        DeviceName = deviceName;
        IpAddress = ipAddress;
    }

    public long UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public long? ReplacedBySessionId { get; private set; }

    public string? DeviceName { get; private set; }

    public string? IpAddress { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static RefreshSession Create(
        long userId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        string? deviceName,
        string? ipAddress)
    {
        return new RefreshSession(userId, tokenHash, expiresAtUtc, createdAtUtc, deviceName, ipAddress);
    }

    public bool IsActive(DateTime nowUtc)
    {
        return RevokedAtUtc is null && ReplacedBySessionId is null && ExpiresAtUtc > nowUtc;
    }

    public Result EnsureCanRefresh(DateTime nowUtc)
    {
        if (RevokedAtUtc is not null || ReplacedBySessionId is not null)
        {
            return Result.Failure(AuthErrors.RefreshSessionReplayed);
        }

        if (ExpiresAtUtc <= nowUtc)
        {
            return Result.Failure(AuthErrors.RefreshSessionExpired);
        }

        return Result.Success();
    }

    public void Revoke(DateTime nowUtc)
    {
        RevokedAtUtc ??= nowUtc;
    }

    public void ReplaceBy(long newSessionId, DateTime nowUtc)
    {
        ReplacedBySessionId = newSessionId;
        Revoke(nowUtc);
    }
}

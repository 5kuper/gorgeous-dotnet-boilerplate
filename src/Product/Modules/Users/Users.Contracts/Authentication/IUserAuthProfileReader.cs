namespace Users.Contracts.Authentication;

public interface IUserAuthProfileReader
{
    Task<UserAuthProfile?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken ct = default);

    Task<UserAuthProfile?> GetByUserIdAsync(
        long userId,
        CancellationToken ct = default);
}


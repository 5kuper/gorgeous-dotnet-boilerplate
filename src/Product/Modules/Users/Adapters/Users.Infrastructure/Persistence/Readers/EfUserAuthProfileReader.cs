using Microsoft.EntityFrameworkCore;
using Users.Contracts.Authentication;

namespace Users.Infrastructure.Persistence.Readers;

internal sealed class EfUserAuthProfileReader(UsersDbContext dbContext) : IUserAuthProfileReader
{
    public Task<UserAuthProfile?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken ct = default)
    {
        return Query().FirstOrDefaultAsync(user => user.PublicId == publicId, ct);
    }

    public Task<UserAuthProfile?> GetByUserIdAsync(
        long userId,
        CancellationToken ct = default)
    {
        return Query().FirstOrDefaultAsync(user => user.UserId == userId, ct);
    }

    private IQueryable<UserAuthProfile> Query()
    {
        return dbContext.Users
            .AsNoTracking()
            .Select(user => new UserAuthProfile(
                user.Id,
                user.PublicId,
                user.Email,
                user.Status.ToString(),
                user.EmailVerified,
                user.Roles
                    .Select(userRole => userRole.Role.Code)
                    .ToArray()));
    }
}


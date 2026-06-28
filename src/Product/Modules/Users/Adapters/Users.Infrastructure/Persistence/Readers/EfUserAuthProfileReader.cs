using Microsoft.EntityFrameworkCore;
using Users.Contracts.Authentication;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Readers;

internal sealed class EfUserAuthProfileReader(UsersDbContext dbContext) : IUserAuthProfileReader
{
    public Task<UserAuthProfile?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken ct = default)
    {
        return Project(dbContext.Users.Where(user => user.PublicId == publicId))
            .FirstOrDefaultAsync(ct);
    }

    public Task<UserAuthProfile?> GetByUserIdAsync(
        long userId,
        CancellationToken ct = default)
    {
        return Project(dbContext.Users.Where(user => user.Id == userId))
            .FirstOrDefaultAsync(ct);
    }

    private static IQueryable<UserAuthProfile> Project(IQueryable<User> users)
    {
        return users
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


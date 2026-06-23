using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return IncludeRoles(dbContext.Users)
            .FirstOrDefaultAsync(user => user.Id == id, ct);
    }

    public Task<User?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default)
    {
        return IncludeRoles(dbContext.Users)
            .FirstOrDefaultAsync(user => user.PublicId == publicId, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        string? normalizedEmail = User.NormalizeEmail(email);

        return IncludeRoles(dbContext.Users)
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await dbContext.Users.AddAsync(user, ct);
    }

    private static IQueryable<User> IncludeRoles(IQueryable<User> query)
    {
        return query
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role);
    }
}


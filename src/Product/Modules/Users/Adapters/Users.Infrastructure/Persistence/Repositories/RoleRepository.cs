using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository(UsersDbContext dbContext) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return dbContext.Roles.FirstOrDefaultAsync(role => role.Id == id, ct);
    }

    public Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        string normalizedCode = Role.NormalizeCode(code);

        return dbContext.Roles.FirstOrDefaultAsync(role => role.Code == normalizedCode, ct);
    }

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        await dbContext.Roles.AddAsync(role, ct);
    }
}


using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(long id, CancellationToken ct = default);

    Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task AddAsync(Role role, CancellationToken ct = default);
}


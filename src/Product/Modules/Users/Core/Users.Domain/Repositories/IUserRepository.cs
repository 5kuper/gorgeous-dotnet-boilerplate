using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken ct = default);

    Task<User?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);
}


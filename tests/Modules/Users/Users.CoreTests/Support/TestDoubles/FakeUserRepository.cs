using Shared.TestKit.TestData;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.CoreTests.Support.TestDoubles;

internal sealed class FakeUserRepository : IUserRepository
{
    private long _nextId = 1;
    private readonly List<User> _users = [];

    public IReadOnlyCollection<User> Users => _users;

    public Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.Id == id));
    }

    public Task<User?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.PublicId == publicId));
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.Email == User.NormalizeEmail(email)));
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        if (user.Id == 0)
        {
            user.SetId(_nextId++);
        }

        _users.Add(user);

        return Task.CompletedTask;
    }

    public void AddExisting(User user)
    {
        if (user.Id == 0)
        {
            user.SetId(_nextId++);
        }

        _users.Add(user);
    }
}

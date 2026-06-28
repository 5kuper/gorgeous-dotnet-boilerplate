using Shared.TestKit.TestData;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.CoreTests.Support.TestDoubles;

internal sealed class FakeRoleRepository : IRoleRepository
{
    private long _nextId = 1;
    private readonly List<Role> _roles = [];

    public IReadOnlyCollection<Role> Roles => _roles;

    public Task<Role?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return Task.FromResult(_roles.FirstOrDefault(role => role.Id == id));
    }

    public Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        string normalizedCode = Role.NormalizeCode(code);

        return Task.FromResult(_roles.FirstOrDefault(role => role.Code == normalizedCode));
    }

    public Task AddAsync(Role role, CancellationToken ct = default)
    {
        if (role.Id == 0)
        {
            role.SetId(_nextId++);
        }

        _roles.Add(role);

        return Task.CompletedTask;
    }

    public void AddExisting(Role role)
    {
        if (role.Id == 0)
        {
            role.SetId(_nextId++);
        }

        _roles.Add(role);
        _nextId = Math.Max(_nextId, role.Id + 1);
    }
}

using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence;

namespace ProjectName.Persistence;

public sealed class DatabaseInitializer(
    UsersDbContext usersDbContext,
    AuthDbContext authDbContext)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await usersDbContext.Database.MigrateAsync(ct);
        await authDbContext.Database.MigrateAsync(ct);
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Shared.IntegrationTesting.Database;

public sealed class SqliteTestDatabase<TContext> : IAsyncDisposable
    where TContext : DbContext
{
    public SqliteConnection Connection { get; } = new("Data Source=:memory:");

    public async Task InitializeAsync(
        Func<DbContextOptions<TContext>, TContext> contextFactory,
        CancellationToken ct = default)
    {
        await Connection.OpenAsync(ct);

        await using var context = CreateContext(contextFactory);
        await context.Database.EnsureCreatedAsync(ct);
    }

    public TContext CreateContext(Func<DbContextOptions<TContext>, TContext> contextFactory)
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(Connection)
            .Options;

        return contextFactory(options);
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}

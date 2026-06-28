using Shared.BuildingBlocks.Application.Abstractions;

namespace Users.CoreTests.Support.TestDoubles;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalls++;

        return Task.CompletedTask;
    }
}

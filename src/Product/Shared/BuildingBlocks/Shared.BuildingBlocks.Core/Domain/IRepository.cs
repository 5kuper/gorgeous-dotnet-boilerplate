namespace Shared.BuildingBlocks.Core.Domain;

public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken ct = default);

    Task AddAsync(TAggregate aggregate, CancellationToken ct = default);

    void Delete(TAggregate aggregate);
}


namespace Shared.BuildingBlocks.Core.Domain;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

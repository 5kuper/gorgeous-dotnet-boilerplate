namespace Shared.Kernel.BuildingBlocks;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

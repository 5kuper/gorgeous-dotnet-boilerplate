using Shared.Kernel.BuildingBlocks;

namespace Users.Domain.Foundation.Events;

public sealed record UserRegisteredDomainEvent(
    long UserId,
    Guid PublicId,
    DateTime OccurredOnUtc) : IDomainEvent;

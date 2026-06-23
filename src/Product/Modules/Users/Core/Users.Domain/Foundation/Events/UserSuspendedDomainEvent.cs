using Shared.BuildingBlocks.Core.Domain;

namespace Users.Domain.Foundation.Events;

public sealed record UserSuspendedDomainEvent(
    long UserId,
    DateTime OccurredOnUtc) : IDomainEvent;

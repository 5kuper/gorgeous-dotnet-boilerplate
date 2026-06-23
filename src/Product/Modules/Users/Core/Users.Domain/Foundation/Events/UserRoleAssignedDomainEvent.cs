using Shared.BuildingBlocks.Core.Domain;

namespace Users.Domain.Foundation.Events;

public sealed record UserRoleAssignedDomainEvent(
    long UserId,
    long RoleId,
    DateTime OccurredOnUtc) : IDomainEvent;

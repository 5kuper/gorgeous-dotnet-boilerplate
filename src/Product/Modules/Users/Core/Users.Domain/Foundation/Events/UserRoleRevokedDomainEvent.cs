using Shared.BuildingBlocks.Core.Domain;

namespace Users.Domain.Foundation.Events;

public sealed record UserRoleRevokedDomainEvent(
    long UserId,
    long RoleId,
    DateTime OccurredOnUtc) : IDomainEvent;

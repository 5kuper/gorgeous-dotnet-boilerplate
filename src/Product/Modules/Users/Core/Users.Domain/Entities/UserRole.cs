using Shared.Kernel.BuildingBlocks;

namespace Users.Domain.Entities;

public sealed class UserRole : Entity<long>
{
    private UserRole()
    {
    }

    internal UserRole(long roleId, DateTime assignedAtUtc, long? assignedByUserId)
    {
        RoleId = roleId;
        AssignedAtUtc = assignedAtUtc;
        AssignedByUserId = assignedByUserId;
    }

    public long UserId { get; private set; }

    public long RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public DateTime AssignedAtUtc { get; private set; }

    public long? AssignedByUserId { get; private set; }
}

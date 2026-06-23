namespace Shared.BuildingBlocks.Application.Abstractions;

public interface ICurrentUser
{
    Guid? PublicUserId { get; }

    bool IsAuthenticated { get; }
}

using Shared.BuildingBlocks.Application.Abstractions;

namespace Shared.IntegrationTesting.Authentication;

public sealed class TestCurrentUser(Guid? publicUserId) : ICurrentUser
{
    public Guid? PublicUserId { get; } = publicUserId;

    public bool IsAuthenticated => PublicUserId is not null;
}

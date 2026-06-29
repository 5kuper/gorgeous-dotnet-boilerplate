using Gorgeous.Abstractions.Application;

namespace Shared.IntegrationTesting.Authentication;

public sealed class TestCurrentUser(Guid? publicUserId) : ICurrentUser
{
    public Guid? PublicUserId { get; } = publicUserId;

    public bool IsAuthenticated => PublicUserId is not null;
}

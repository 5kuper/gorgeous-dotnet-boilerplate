namespace Gorgeous.Abstractions.Application;

public interface ICurrentUser
{
    Guid? PublicUserId { get; }

    bool IsAuthenticated { get; }
}


namespace Gorgeous.Abstractions.Application;

public interface IClock
{
    DateTime UtcNow { get; }
}


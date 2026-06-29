using Gorgeous.Abstractions.Application;

namespace Gorgeous.Web;

public sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}


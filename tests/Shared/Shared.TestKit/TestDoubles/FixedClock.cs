using Gorgeous.Abstractions.Application;

namespace Shared.TestKit.TestDoubles;

public sealed class FixedClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; set; } = utcNow;
}

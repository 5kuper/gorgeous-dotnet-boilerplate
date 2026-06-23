using Shared.BuildingBlocks.Application.Abstractions;

namespace Shared.WebFramework;

public sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}

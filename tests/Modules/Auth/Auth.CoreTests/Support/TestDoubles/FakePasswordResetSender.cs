using Auth.Application.Ports.Messaging;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakePasswordResetSender : IPasswordResetSender
{
    public int SendCalls { get; private set; }

    public Task<Result> SendAsync(string email, string token, CancellationToken ct = default)
    {
        SendCalls++;

        return Task.FromResult(Result.Success());
    }
}

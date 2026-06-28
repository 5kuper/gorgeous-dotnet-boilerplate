using Auth.Application.Ports.Messaging;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeEmailConfirmationSender : IEmailConfirmationSender
{
    public int SendCalls { get; private set; }

    public string? LastEmail { get; private set; }

    public string? LastCode { get; private set; }

    public Task<Result> SendAsync(string email, string code, CancellationToken ct = default)
    {
        SendCalls++;
        LastEmail = email;
        LastCode = code;

        return Task.FromResult(Result.Success());
    }
}

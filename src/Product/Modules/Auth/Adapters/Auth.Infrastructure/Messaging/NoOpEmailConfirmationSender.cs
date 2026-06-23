using Auth.Application.Ports.Messaging;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.Infrastructure.Messaging;

internal sealed class NoOpEmailConfirmationSender(
    ILogger<NoOpEmailConfirmationSender> logger) : IEmailConfirmationSender
{
    public Task<Result> SendAsync(
        string email,
        string code,
        CancellationToken ct = default)
    {
        logger.LogInformation("Email confirmation code generated for {Email}.", email);

        return Task.FromResult(Result.Success());
    }
}


using Auth.Application.Ports.Messaging;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.Infrastructure.Messaging;

internal sealed class NoOpPasswordResetSender(
    ILogger<NoOpPasswordResetSender> logger) : IPasswordResetSender
{
    public Task<Result> SendAsync(
        string email,
        string token,
        CancellationToken ct = default)
    {
        logger.LogInformation("Password reset token generated for {Email}.", email);

        return Task.FromResult(Result.Success());
    }
}


using Shared.BuildingBlocks.Core.Results;

namespace Auth.Application.Ports.Messaging;

public interface IPasswordResetSender
{
    Task<Result> SendAsync(
        string email,
        string token,
        CancellationToken ct = default);
}


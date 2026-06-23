using Shared.BuildingBlocks.Core.Results;

namespace Auth.Application.Ports.Messaging;

public interface IEmailConfirmationSender
{
    Task<Result> SendAsync(
        string email,
        string code,
        CancellationToken ct = default);
}


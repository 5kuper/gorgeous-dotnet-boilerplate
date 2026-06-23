using Auth.Application.Features.Sessions.Support;
using Auth.Domain.Repositories;
using Shared.BuildingBlocks.Application.Abstractions;
using Shared.BuildingBlocks.Application.Messaging;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.Application.Features.Sessions;

public sealed record LogoutCommand(string RefreshToken) : ICommand;

internal sealed class LogoutCommandHandler(
    IRefreshSessionRepository refreshSessionRepository,
    IClock clock) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var session = await refreshSessionRepository.GetByTokenHashAsync(
            TokenHashing.Sha256(request.RefreshToken),
            ct);

        if (session is null)
        {
            return Result.Success();
        }

        session.Revoke(clock.UtcNow);
        await refreshSessionRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}

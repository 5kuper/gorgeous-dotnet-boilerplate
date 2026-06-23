using Auth.Application.Ports.Identity;
using Auth.Domain.Repositories;
using Shared.BuildingBlocks.Application.Abstractions;
using Shared.BuildingBlocks.Application.Messaging;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.Application.Features.PasswordReset;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand;

internal sealed class ResetPasswordCommandHandler(
    IIdentityCredentialService identityCredentialService,
    IRefreshSessionRepository refreshSessionRepository,
    IClock clock) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var resetResult = await identityCredentialService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword,
            ct);

        if (resetResult.IsFailure)
        {
            return Result.Failure(resetResult.Error);
        }

        await refreshSessionRepository.RevokeAllActiveSessionsForUserAsync(
            resetResult.Value,
            clock.UtcNow,
            ct);
        await refreshSessionRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}

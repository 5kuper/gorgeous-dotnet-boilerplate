using Auth.Application.Ports.Identity;
using Auth.Application.Ports.Messaging;
using Shared.AppModel.Messaging;
using Gorgeous.Abstractions.Results;

namespace Auth.Application.Features.PasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand;

internal sealed class RequestPasswordResetCommandHandler(
    IIdentityCredentialService identityCredentialService,
    IPasswordResetSender passwordResetSender) : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken ct)
    {
        var tokenResult = await identityCredentialService.GeneratePasswordResetTokenAsync(
            request.Email,
            ct);

        if (tokenResult.IsSuccess && tokenResult.Value is not null)
        {
            await passwordResetSender.SendAsync(request.Email, tokenResult.Value, ct);
        }

        return Result.Success();
    }
}

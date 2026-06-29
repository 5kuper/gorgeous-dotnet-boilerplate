using Auth.Application.Ports.Identity;
using Auth.Application.Ports.Verification;
using Shared.AppModel.Messaging;
using Gorgeous.Abstractions.Results;
using Users.Contracts.Verification;

namespace Auth.Application.Features.Verification;

public sealed record ConfirmEmailCommand(string Code) : ICommand;

internal sealed class ConfirmEmailCommandHandler(
    IIdentityCredentialService identityCredentialService,
    IEmailConfirmationCodeProtector emailConfirmationCodeProtector,
    IUsersEmailVerification usersEmailVerification) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var payloadResult = emailConfirmationCodeProtector.Unprotect(request.Code);

        if (payloadResult.IsFailure)
        {
            return Result.Failure(payloadResult.Error);
        }

        var payload = payloadResult.Value;

        var identityResult = await identityCredentialService.ConfirmEmailAsync(
            payload.UserPublicId,
            payload.Token,
            ct);

        if (identityResult.IsFailure)
        {
            return identityResult;
        }

        return await usersEmailVerification.ConfirmEmailAsync(payload.UserPublicId, ct);
    }
}

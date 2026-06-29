using MediatR;
using Gorgeous.Abstractions.Results;
using Users.Contracts.Verification;

namespace Users.Application.Features.EmailVerification;

internal sealed class UsersEmailConfirmationService(ISender sender) : IUsersEmailVerification
{
    public Task<Result> ConfirmEmailAsync(Guid userPublicId, CancellationToken ct = default)
    {
        return sender.Send(new ConfirmUserEmailCommand(userPublicId), ct);
    }
}


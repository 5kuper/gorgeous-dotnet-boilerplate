using Gorgeous.Abstractions.Application;
using Shared.AppModel.Abstractions;
using Shared.AppModel.Messaging;
using Gorgeous.Abstractions.Results;
using Users.Domain.Foundation.Errors;
using Users.Domain.Repositories;

namespace Users.Application.Features.Administration;

public sealed record SuspendUserCommand(Guid UserPublicId) : ICommand;

internal sealed class SuspendUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SuspendUserCommand>
{
    public async Task<Result> Handle(SuspendUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByPublicIdAsync(request.UserPublicId, ct);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var result = user.Suspend(clock.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

using Gorgeous.Abstractions.Application;
using Shared.AppModel.Abstractions;
using Shared.AppModel.Messaging;
using Gorgeous.Abstractions.Results;
using Users.Domain.Foundation.Errors;
using Users.Domain.Repositories;

namespace Users.Application.Features.Profile;

public sealed record UpdateProfileCommand(Guid UserPublicId, string DisplayName) : ICommand;

internal sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByPublicIdAsync(request.UserPublicId, ct);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var result = user.UpdateProfile(request.DisplayName, clock.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

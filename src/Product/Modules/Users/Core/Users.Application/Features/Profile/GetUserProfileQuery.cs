using Shared.BuildingBlocks.Application.Messaging;
using Shared.BuildingBlocks.Core.Results;
using Users.Domain.Foundation.Errors;
using Users.Domain.Repositories;

namespace Users.Application.Features.Profile;

public sealed record GetUserProfileQuery(Guid UserPublicId) : IQuery<UserProfile>;

public sealed record UserProfile(
    Guid PublicId,
    string? Email,
    string DisplayName,
    string Status,
    bool EmailVerified,
    IReadOnlyCollection<string> Roles);

internal sealed class GetUserProfileQueryHandler(
    IUserRepository userRepository) : IQueryHandler<GetUserProfileQuery, UserProfile>
{
    public async Task<Result<UserProfile>> Handle(
        GetUserProfileQuery request,
        CancellationToken ct)
    {
        var user = await userRepository.GetByPublicIdAsync(request.UserPublicId, ct);

        if (user is null)
        {
            return Result.Failure<UserProfile>(UserErrors.NotFound);
        }

        return Result.Success(new UserProfile(
            user.PublicId,
            user.Email,
            user.DisplayName,
            user.Status.ToString(),
            user.EmailVerified,
            user.Roles
                .Where(userRole => userRole.Role is not null)
                .Select(userRole => userRole.Role.Code)
                .ToArray()));
    }
}

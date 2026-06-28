using Users.Contracts.Authentication;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeUserAuthProfileReader : IUserAuthProfileReader
{
    private readonly Dictionary<long, UserAuthProfile> _profilesByUserId = [];
    private readonly Dictionary<Guid, UserAuthProfile> _profilesByPublicId = [];

    public Task<UserAuthProfile?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default)
    {
        _profilesByPublicId.TryGetValue(publicId, out var profile);

        return Task.FromResult(profile);
    }

    public Task<UserAuthProfile?> GetByUserIdAsync(long userId, CancellationToken ct = default)
    {
        _profilesByUserId.TryGetValue(userId, out var profile);

        return Task.FromResult(profile);
    }

    public void Add(UserAuthProfile profile)
    {
        _profilesByUserId[profile.UserId] = profile;
        _profilesByPublicId[profile.PublicId] = profile;
    }
}

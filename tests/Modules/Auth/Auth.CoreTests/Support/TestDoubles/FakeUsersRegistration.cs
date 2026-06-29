using Gorgeous.Abstractions.Results;
using Users.Contracts.Registration;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeUsersRegistration : IUsersRegistration
{
    public Result<RegisteredUser> RegistrationResult { get; set; } = Result.Success(
        new RegisteredUser(
            UserId: 10,
            PublicId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email: "user@example.com",
            DisplayName: "Test User",
            Status: "PendingActivation"));

    public RegisterUser? LastRequest { get; private set; }

    public Task<Result<RegisteredUser>> RegisterAsync(
        RegisterUser request,
        CancellationToken ct = default)
    {
        LastRequest = request;

        return Task.FromResult(RegistrationResult);
    }
}

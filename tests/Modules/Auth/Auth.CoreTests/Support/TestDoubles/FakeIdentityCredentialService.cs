using Auth.Application.Ports.Identity;
using Shared.BuildingBlocks.Core.Results;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeIdentityCredentialService : IIdentityCredentialService
{
    public Result CreateUserResult { get; set; } = Result.Success();

    public CredentialSignInResult SignInResult { get; set; } = new(true, false, 10);

    public Result<string> EmailConfirmationTokenResult { get; set; } = Result.Success("email-token");

    public Result ConfirmEmailResult { get; set; } = Result.Success();

    public Result<string?> PasswordResetTokenResult { get; set; } = Result.Success<string?>("reset-token");

    public Result<long> ResetPasswordResult { get; set; } = Result.Success(10L);

    public int CreateUserCalls { get; private set; }

    public int GenerateEmailConfirmationTokenCalls { get; private set; }

    public int ConfirmEmailCalls { get; private set; }

    public int ResetPasswordCalls { get; private set; }

    public Task<Result> CreateUserAsync(
        long userId,
        Guid userPublicId,
        string email,
        string password,
        CancellationToken ct = default)
    {
        CreateUserCalls++;

        return Task.FromResult(CreateUserResult);
    }

    public Task<CredentialSignInResult> CheckPasswordSignInByEmailAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        return Task.FromResult(SignInResult);
    }

    public Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userPublicId,
        CancellationToken ct = default)
    {
        GenerateEmailConfirmationTokenCalls++;

        return Task.FromResult(EmailConfirmationTokenResult);
    }

    public Task<Result> ConfirmEmailAsync(
        Guid userPublicId,
        string token,
        CancellationToken ct = default)
    {
        ConfirmEmailCalls++;

        return Task.FromResult(ConfirmEmailResult);
    }

    public Task<Result<string?>> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken ct = default)
    {
        return Task.FromResult(PasswordResetTokenResult);
    }

    public Task<Result<long>> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken ct = default)
    {
        ResetPasswordCalls++;

        return Task.FromResult(ResetPasswordResult);
    }
}

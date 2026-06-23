using Shared.BuildingBlocks.Core.Results;

namespace Auth.Application.Ports.Identity;

public interface IIdentityCredentialService
{
    Task<Result> CreateUserAsync(
        long userId,
        Guid userPublicId,
        string email,
        string password,
        CancellationToken ct = default);

    Task<CredentialSignInResult> CheckPasswordSignInByEmailAsync(
        string email,
        string password,
        CancellationToken ct = default);

    Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userPublicId,
        CancellationToken ct = default);

    Task<Result> ConfirmEmailAsync(
        Guid userPublicId,
        string token,
        CancellationToken ct = default);

    Task<Result<string?>> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken ct = default);

    Task<Result<long>> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken ct = default);
}



using Auth.Application.Ports.Identity;
using Auth.Domain.Foundation.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Gorgeous.Abstractions.Results;

namespace Auth.Infrastructure.Identity;

internal sealed class IdentityCredentialService(
    UserManager<AppIdentityUser> userManager,
    SignInManager<AppIdentityUser> signInManager) : IIdentityCredentialService
{
    public async Task<Result> CreateUserAsync(
        long userId,
        Guid userPublicId,
        string email,
        string password,
        CancellationToken ct = default)
    {
        var identityUser = new AppIdentityUser
        {
            UserId = userId,
            UserPublicId = userPublicId,
            Email = email,
            UserName = email,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(identityUser, password);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapIdentityError(result));
    }

    public async Task<CredentialSignInResult> CheckPasswordSignInByEmailAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        var identityUser = await userManager.FindByEmailAsync(email);

        if (identityUser is null)
        {
            return new CredentialSignInResult(false, false, null);
        }

        var result = await signInManager.CheckPasswordSignInAsync(
            identityUser,
            password,
            lockoutOnFailure: true);

        return new CredentialSignInResult(result.Succeeded, result.IsNotAllowed, identityUser.UserId);
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userPublicId,
        CancellationToken ct = default)
    {
        var identityUser = await FindByBusinessUserPublicIdAsync(userPublicId, ct);

        if (identityUser is null)
        {
            return Result.Failure<string>(AuthErrors.UserNotAllowed);
        }

        string token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);

        return Result.Success(token);
    }

    public async Task<Result> ConfirmEmailAsync(
        Guid userPublicId,
        string token,
        CancellationToken ct = default)
    {
        var identityUser = await FindByBusinessUserPublicIdAsync(userPublicId, ct);

        if (identityUser is null)
        {
            return Result.Failure(AuthErrors.UserNotAllowed);
        }

        if (identityUser.EmailConfirmed)
        {
            return Result.Success();
        }

        var result = await userManager.ConfirmEmailAsync(identityUser, token);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapIdentityError(result));
    }

    public async Task<Result<string?>> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken ct = default)
    {
        var identityUser = await userManager.FindByEmailAsync(email);

        if (identityUser is null)
        {
            return Result.Success<string?>(null);
        }

        return Result.Success<string?>(await userManager.GeneratePasswordResetTokenAsync(identityUser));
    }

    public async Task<Result<long>> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken ct = default)
    {
        var identityUser = await userManager.FindByEmailAsync(email);

        if (identityUser is null)
        {
            return Result.Failure<long>(AuthErrors.PasswordResetFailed);
        }

        var result = await userManager.ResetPasswordAsync(identityUser, token, newPassword);

        return result.Succeeded
            ? Result.Success(identityUser.UserId)
            : Result.Failure<long>(AuthErrors.PasswordResetFailed);
    }

    private Task<AppIdentityUser?> FindByBusinessUserIdAsync(
        long userId,
        CancellationToken ct)
    {
        return userManager.Users.FirstOrDefaultAsync(user => user.UserId == userId, ct);
    }

    private Task<AppIdentityUser?> FindByBusinessUserPublicIdAsync(
        Guid userPublicId,
        CancellationToken ct)
    {
        return userManager.Users.FirstOrDefaultAsync(user => user.UserPublicId == userPublicId, ct);
    }

    private static Error MapIdentityError(IdentityResult result)
    {
        var error = result.Errors.FirstOrDefault();

        if (error is null)
        {
            return AuthErrors.IdentityOperationFailed;
        }

        var type = error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)
            ? ErrorType.Conflict
            : ErrorType.Validation;

        return new Error("Auth.IdentityOperationFailed", "Identity operation failed.", type);
    }
}


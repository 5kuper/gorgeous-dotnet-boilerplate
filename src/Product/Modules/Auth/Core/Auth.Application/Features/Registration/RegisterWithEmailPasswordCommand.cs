using Auth.Application.Features.Sessions.Support;
using Auth.Application.Ports.Identity;
using Auth.Application.Ports.Messaging;
using Auth.Application.Ports.Registration;
using Auth.Application.Ports.Verification;
using Shared.BuildingBlocks.Application.Messaging;
using Shared.BuildingBlocks.Core.Results;
using Users.Contracts.Registration;

namespace Auth.Application.Features.Registration;

public sealed record RegisterWithEmailPasswordCommand(
    string Email,
    string Password,
    string DisplayName) : ICommand<RegistrationResult>;

internal sealed class RegisterWithEmailPasswordCommandHandler(
    IRegistrationTransaction registrationTransaction,
    IUsersRegistration usersRegistration,
    IIdentityCredentialService identityCredentialService,
    IEmailConfirmationCodeProtector emailConfirmationCodeProtector,
    IEmailConfirmationSender emailConfirmationSender) : ICommandHandler<RegisterWithEmailPasswordCommand, RegistrationResult>
{
    public async Task<Result<RegistrationResult>> Handle(
        RegisterWithEmailPasswordCommand request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Result.Failure<RegistrationResult>(new Error(
                "Auth.InvalidRegistration",
                "Registration data is invalid.",
                ErrorType.Validation));
        }

        var registeredUserResult = await registrationTransaction.ExecuteAsync(
            async operationCt =>
            {
                var userResult = await usersRegistration.RegisterAsync(
                    new RegisterUser(
                        request.Email,
                        request.DisplayName,
                        "EmailPassword",
                        AuthRules.RequireConfirmedEmailForPasswordLogin),
                    operationCt);

                if (userResult.IsFailure)
                {
                    return userResult;
                }

                var credentialResult = await identityCredentialService.CreateUserAsync(
                    userResult.Value.UserId,
                    userResult.Value.PublicId,
                    request.Email,
                    request.Password,
                    operationCt);

                return credentialResult.IsFailure
                    ? Result.Failure<RegisteredUser>(credentialResult.Error)
                    : userResult;
            },
            ct);

        if (registeredUserResult.IsFailure)
        {
            return Result.Failure<RegistrationResult>(registeredUserResult.Error);
        }

        if (AuthRules.RequireConfirmedEmailForPasswordLogin)
        {
            var tokenResult = await identityCredentialService.GenerateEmailConfirmationTokenAsync(
                registeredUserResult.Value.PublicId,
                ct);

            if (tokenResult.IsFailure)
            {
                return Result.Failure<RegistrationResult>(tokenResult.Error);
            }

            var codeResult = emailConfirmationCodeProtector.Protect(
                new EmailConfirmationPayload(
                    registeredUserResult.Value.PublicId,
                    tokenResult.Value));

            if (codeResult.IsFailure)
            {
                return Result.Failure<RegistrationResult>(codeResult.Error);
            }

            if (registeredUserResult.Value.Email is not null)
            {
                var sendResult = await emailConfirmationSender.SendAsync(
                    registeredUserResult.Value.Email,
                    codeResult.Value,
                    ct);

                if (sendResult.IsFailure)
                {
                    return Result.Failure<RegistrationResult>(sendResult.Error);
                }
            }
        }

        return Result.Success(new RegistrationResult(
            registeredUserResult.Value.PublicId,
            registeredUserResult.Value.Email,
            AuthRules.RequireConfirmedEmailForPasswordLogin));
    }
}

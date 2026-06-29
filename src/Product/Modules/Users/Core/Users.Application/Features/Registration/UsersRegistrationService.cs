using MediatR;
using Gorgeous.Abstractions.Results;
using Users.Contracts.Registration;

namespace Users.Application.Features.Registration;

internal sealed class UsersRegistrationService(ISender sender) : IUsersRegistration
{
    public Task<Result<RegisteredUser>> RegisterAsync(
        RegisterUser request,
        CancellationToken ct = default)
    {
        return sender.Send(
            new RegisterUserCommand(
                request.Email,
                request.DisplayName,
                request.RegistrationMethod,
                request.RequireEmailConfirmation),
            ct);
    }
}


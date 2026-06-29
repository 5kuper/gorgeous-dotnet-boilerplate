using Gorgeous.Abstractions.Results;

namespace Auth.Application.Ports.Verification;

public interface IEmailConfirmationCodeProtector
{
    Result<string> Protect(EmailConfirmationPayload payload);

    Result<EmailConfirmationPayload> Unprotect(string code);
}

using Auth.Application.Ports.Verification;
using Gorgeous.Abstractions.Results;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeEmailConfirmationCodeProtector : IEmailConfirmationCodeProtector
{
    public Result<string> ProtectResult { get; set; } = Result.Success("confirmation-code");

    public Result<EmailConfirmationPayload> UnprotectResult { get; set; } = Result.Success(
        new EmailConfirmationPayload(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "email-token"));

    public Result<string> Protect(EmailConfirmationPayload payload)
    {
        return ProtectResult;
    }

    public Result<EmailConfirmationPayload> Unprotect(string code)
    {
        return UnprotectResult;
    }
}

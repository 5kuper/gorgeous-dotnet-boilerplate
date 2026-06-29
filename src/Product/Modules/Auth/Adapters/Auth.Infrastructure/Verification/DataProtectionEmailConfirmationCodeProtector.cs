using System.Security.Cryptography;
using System.Text.Json;
using Auth.Application.Ports.Verification;
using Auth.Domain.Foundation.Errors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Gorgeous.Abstractions.Results;

namespace Auth.Infrastructure.Verification;

internal sealed class DataProtectionEmailConfirmationCodeProtector : IEmailConfirmationCodeProtector
{
    private const string Purpose = "Auth.EmailConfirmationCode.v1";

    private readonly IDataProtector _protector;

    public DataProtectionEmailConfirmationCodeProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public Result<string> Protect(EmailConfirmationPayload payload)
    {
        if (payload.UserPublicId == Guid.Empty || string.IsNullOrWhiteSpace(payload.Token))
        {
            return Result.Failure<string>(AuthErrors.InvalidEmailConfirmationCode);
        }

        byte[] json = JsonSerializer.SerializeToUtf8Bytes(payload);
        byte[] protectedPayload = _protector.Protect(json);

        return Result.Success(Base64UrlTextEncoder.Encode(protectedPayload));
    }

    public Result<EmailConfirmationPayload> Unprotect(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<EmailConfirmationPayload>(AuthErrors.InvalidEmailConfirmationCode);
        }

        try
        {
            byte[] protectedPayload = Base64UrlTextEncoder.Decode(code);
            byte[] json = _protector.Unprotect(protectedPayload);
            var payload = JsonSerializer.Deserialize<EmailConfirmationPayload>(json);

            if (payload is null
                || payload.UserPublicId == Guid.Empty
                || string.IsNullOrWhiteSpace(payload.Token))
            {
                return Result.Failure<EmailConfirmationPayload>(AuthErrors.InvalidEmailConfirmationCode);
            }

            return Result.Success(payload);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or JsonException)
        {
            return Result.Failure<EmailConfirmationPayload>(AuthErrors.InvalidEmailConfirmationCode);
        }
    }
}

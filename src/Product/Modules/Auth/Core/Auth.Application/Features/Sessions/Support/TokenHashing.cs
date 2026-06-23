using System.Security.Cryptography;
using System.Text;

namespace Auth.Application.Features.Sessions.Support;

internal static class TokenHashing
{
    public static string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string Sha256(string token)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hash);
    }
}

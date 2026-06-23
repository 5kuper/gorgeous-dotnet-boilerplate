using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Application.Ports.Tokens;
using Auth.Contracts.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.BuildingBlocks.Application.Abstractions;
using Users.Contracts.Authentication;

namespace Auth.Infrastructure.Tokens;

internal sealed class JwtTokenIssuer(IOptions<AuthTokenOptions> options, IClock clock) : ITokenIssuer
{
    public IssuedAccessToken IssueAccessToken(UserAuthProfile profile, long? sessionId)
    {
        var tokenOptions = options.Value;

        if (string.IsNullOrWhiteSpace(tokenOptions.SigningKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var nowUtc = clock.UtcNow;
        var expiresAtUtc = nowUtc.AddMinutes(tokenOptions.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, profile.PublicId.ToString()),
            new(ClaimTypes.NameIdentifier, profile.PublicId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AuthClaimTypes.EmailVerified, profile.EmailVerified.ToString().ToLowerInvariant())
        };

        if (profile.Email is not null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, profile.Email));
        }

        if (sessionId is not null)
        {
            claims.Add(new Claim("sid", sessionId.Value.ToString()));
        }

        claims.AddRange(profile.Roles.Select(role => new Claim(AuthClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            tokenOptions.Issuer,
            tokenOptions.Audience,
            claims,
            nowUtc,
            expiresAtUtc,
            credentials);

        return new IssuedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}

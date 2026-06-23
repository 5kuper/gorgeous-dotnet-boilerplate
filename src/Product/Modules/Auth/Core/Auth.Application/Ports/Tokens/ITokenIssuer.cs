using Users.Contracts.Authentication;

namespace Auth.Application.Ports.Tokens;

public interface ITokenIssuer
{
    IssuedAccessToken IssueAccessToken(UserAuthProfile profile, long? sessionId);
}

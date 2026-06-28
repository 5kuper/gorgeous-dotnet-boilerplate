using Auth.Application.Ports.Tokens;
using Users.Contracts.Authentication;

namespace Auth.CoreTests.Support.TestDoubles;

internal sealed class FakeTokenIssuer : ITokenIssuer
{
    public int IssueAccessTokenCalls { get; private set; }

    public IssuedAccessToken IssueAccessToken(UserAuthProfile profile, long? sessionId)
    {
        IssueAccessTokenCalls++;

        return new IssuedAccessToken("access-token", new DateTime(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc));
    }
}

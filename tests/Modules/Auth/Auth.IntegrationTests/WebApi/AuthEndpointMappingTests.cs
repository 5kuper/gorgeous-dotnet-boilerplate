using Auth.IntegrationTests.WebApi.Support;
using Shared.IntegrationTesting.WebApi;
using Shared.Conventions;

namespace Auth.IntegrationTests.WebApi;

public sealed class AuthEndpointMappingTests
{
    [Fact]
    public void MapAuthWebApi_MapsAuthEndpoints()
    {
        using var app = AuthEndpointTestApp.Create();

        var endpoints = app.GetRouteEndpoints();

        endpoints.ShouldMapRoute("/api/auth/register");
        endpoints.ShouldMapRoute("/api/auth/login");
        endpoints.ShouldMapRoute("/api/auth/refresh");
        endpoints.ShouldMapRoute("/api/auth/logout");
        endpoints.ShouldMapRoute("/api/auth/confirm-email");
        endpoints.ShouldMapRoute("/api/auth/password-reset/request");
        endpoints.ShouldMapRoute("/api/auth/password-reset/reset");
    }

    [Fact]
    public void MapAuthWebApi_AppliesAuthorizationAndRateLimitMetadata()
    {
        using var app = AuthEndpointTestApp.Create();
        var endpoints = app.GetRouteEndpoints();

        endpoints.ShouldRequireAuthorization("/api/auth/logout");
        endpoints.ShouldUseRateLimitPolicy("/api/auth/register", RateLimitPolicies.AuthRegister);
        endpoints.ShouldUseRateLimitPolicy("/api/auth/login", RateLimitPolicies.AuthLogin);
    }
}

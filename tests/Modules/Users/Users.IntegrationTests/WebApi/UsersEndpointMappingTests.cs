using Shared.IntegrationTesting.WebApi;
using Shared.WebFramework;
using Users.IntegrationTests.WebApi.Support;

namespace Users.IntegrationTests.WebApi;

public sealed class UsersEndpointMappingTests
{
    [Fact]
    public void MapUsersWebApi_MapsCurrentUserEndpointsWithAuthorization()
    {
        using var app = UsersEndpointTestApp.Create();
        var endpoints = app.GetRouteEndpoints();

        endpoints.ShouldRequireAuthorization("/api/users/me");
        endpoints.ShouldRequireAuthorization("/api/users/admin/roles", AuthorizationPolicies.Admin);
        endpoints.ShouldRequireAuthorization("/api/users/admin/{publicId:guid}/suspend", AuthorizationPolicies.Admin);
    }
}

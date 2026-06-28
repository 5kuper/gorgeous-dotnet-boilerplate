using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;

namespace Shared.IntegrationTesting.WebApi;

public static class EndpointMetadataAssertions
{
    public static IReadOnlyCollection<RouteEndpoint> GetRouteEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        return endpointRouteBuilder
            .DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
    }

    public static IReadOnlyCollection<RouteEndpoint> ShouldMapRoute(
        this IReadOnlyCollection<RouteEndpoint> endpoints,
        string route)
    {
        var routeEndpoints = endpoints
            .Where(endpoint => endpoint.RoutePattern.RawText == route)
            .ToArray();

        if (routeEndpoints.Length == 0)
        {
            throw new InvalidOperationException($"Expected route '{route}' to be mapped.");
        }

        return routeEndpoints;
    }

    public static void ShouldRequireAuthorization(
        this IReadOnlyCollection<RouteEndpoint> endpoints,
        string route,
        string? policy = null)
    {
        var routeEndpoints = endpoints.ShouldMapRoute(route);

        foreach (var endpoint in routeEndpoints)
        {
            var authorization = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();

            if (authorization.Count == 0)
            {
                throw new InvalidOperationException($"Expected route '{route}' to require authorization.");
            }

            if (policy is not null && !authorization.Any(data => data.Policy == policy))
            {
                throw new InvalidOperationException(
                    $"Expected route '{route}' to require authorization policy '{policy}'.");
            }
        }
    }

    public static void ShouldUseRateLimitPolicy(
        this IReadOnlyCollection<RouteEndpoint> endpoints,
        string route,
        string policyName)
    {
        var routeEndpoints = endpoints.ShouldMapRoute(route);

        foreach (var endpoint in routeEndpoints)
        {
            var metadata = endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>();

            if (metadata is null)
            {
                throw new InvalidOperationException($"Expected route '{route}' to use rate limiting.");
            }

            if (metadata.PolicyName != policyName)
            {
                throw new InvalidOperationException(
                    $"Expected route '{route}' to use rate limit policy '{policyName}', but found '{metadata.PolicyName}'.");
            }
        }
    }
}

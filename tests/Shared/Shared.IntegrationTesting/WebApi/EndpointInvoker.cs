using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace Shared.IntegrationTesting.WebApi;

public static class EndpointInvoker
{
    public static async Task<ProblemDetailsSnapshot> InvokePostProblemAsync(
        this WebApplication app,
        string route,
        string jsonBody)
    {
        var endpoint = app
            .GetRouteEndpoints()
            .Single(endpoint => endpoint.RoutePattern.RawText == route);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = app.Services
        };
        httpContext.SetEndpoint(endpoint);
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = route;
        httpContext.Request.RouteValues = new RouteValueDictionary();
        httpContext.Request.ContentType = "application/json";
        byte[] requestBody = Encoding.UTF8.GetBytes(jsonBody);
        httpContext.Request.ContentLength = requestBody.Length;
        httpContext.Request.Body = new MemoryStream(requestBody);
        httpContext.Features.Set<IHttpRequestBodyDetectionFeature>(new RequestBodyDetectionFeature());
        httpContext.Response.Body = new MemoryStream();

        await endpoint.RequestDelegate!(httpContext);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, leaveOpen: true);
        string body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidOperationException(
                $"Expected problem response body for route '{route}', but body was empty. Status: {httpContext.Response.StatusCode}.");
        }

        httpContext.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<JsonElement>(httpContext.Response.Body);

        return new ProblemDetailsSnapshot(
            httpContext.Response.StatusCode,
            problem.GetProperty("detail").GetString(),
            problem.GetProperty("code").GetString());
    }

    private sealed class RequestBodyDetectionFeature : IHttpRequestBodyDetectionFeature
    {
        public bool CanHaveBody => true;
    }
}

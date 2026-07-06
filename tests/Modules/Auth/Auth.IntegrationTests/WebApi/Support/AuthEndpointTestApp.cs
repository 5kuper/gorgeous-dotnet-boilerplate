using Auth.Presentation.WebApi;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.IntegrationTesting.Messaging;

namespace Auth.IntegrationTests.WebApi.Support;

internal static class AuthEndpointTestApp
{
    public static WebApplication Create(ISender? sender = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<ISender>(sender ?? TestSender.Throwing());

        var app = builder.Build();
        app.MapAuthWebApi();

        return app;
    }
}

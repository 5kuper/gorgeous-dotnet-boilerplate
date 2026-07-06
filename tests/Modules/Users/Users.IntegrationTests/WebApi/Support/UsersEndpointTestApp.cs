using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Gorgeous.Abstractions.Application;
using Shared.IntegrationTesting.Authentication;
using Shared.IntegrationTesting.Messaging;
using Users.Presentation.WebApi;

namespace Users.IntegrationTests.WebApi.Support;

internal static class UsersEndpointTestApp
{
    public static WebApplication Create(ISender? sender = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<ISender>(sender ?? TestSender.Throwing());
        builder.Services.AddSingleton<ICurrentUser>(new TestCurrentUser(Guid.NewGuid()));

        var app = builder.Build();
        app.MapUsersWebApi();

        return app;
    }
}

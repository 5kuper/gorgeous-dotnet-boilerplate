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
    public static WebApplication Create()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<ISender, ThrowingSender>();
        builder.Services.AddSingleton<ICurrentUser>(new TestCurrentUser(Guid.NewGuid()));

        var app = builder.Build();
        app.MapUsersWebApi();

        return app;
    }
}

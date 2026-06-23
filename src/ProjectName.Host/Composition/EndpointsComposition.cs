using Auth.Presentation.WebApi;
using Users.Presentation.WebApi;

namespace ProjectName.Host.Composition;

internal static class EndpointsComposition
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Ok(new { service = "ProjectName.Host" }));

        app.MapAuthWebApi();
        app.MapUsersWebApi();

        return app;
    }
}

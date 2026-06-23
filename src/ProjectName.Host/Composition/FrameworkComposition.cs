using Shared.BuildingBlocks.Application.Abstractions;
using Shared.WebFramework;

namespace ProjectName.Host.Composition;

internal static class FrameworkComposition
{
    public static IServiceCollection AddFramework(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddValidation();
        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        return services;
    }

    public static WebApplication UseFramework(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        return app;
    }
}

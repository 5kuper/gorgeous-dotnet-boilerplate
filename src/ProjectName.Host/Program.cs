using ProjectName.Host.Composition;
using ProjectName.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFramework();

builder.Services.AddModules(builder.Configuration);
builder.Services.AddSecurity();
builder.Services.AddRateLimiting();

var app = builder.Build();

if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Persistence:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();

    await scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>()
        .InitializeAsync(app.Lifetime.ApplicationStopping);
}

app.UseFramework();
app.UseSecurity();

app.MapEndpoints();

app.Run();

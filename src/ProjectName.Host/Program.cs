using Microsoft.Extensions.Options;
using ProjectName.Host.Composition;
using ProjectName.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddProjectConfiguration(args);

builder.Services.AddConfigurationServices(builder.Configuration);
builder.Services.AddFramework();

builder.Services.AddModules(builder.Configuration);
builder.Services.AddSecurity();
builder.Services.AddRateLimiting();

var app = builder.Build();

var persistenceOptions = app.Services.GetRequiredService<IOptions<PersistenceOptions>>().Value;

if (app.Environment.IsDevelopment() ||
    persistenceOptions.ApplyMigrationsOnStartup)
{
    await using var scope = app.Services.CreateAsyncScope();

    await scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>()
        .InitializeAsync(app.Lifetime.ApplicationStopping);
}

app.UseProjectConfiguration();
app.UseFramework();
app.UseSecurity();

app.MapEndpoints();

app.Run();

using Auth.Infrastructure;
using Auth.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.IntegrationTests.Persistence.Support;

internal static class AuthPersistenceServices
{
    public static ServiceProvider CreateServiceProvider(AuthDbContext context)
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddAuthInfra(configuration);
        services.AddSingleton(context);

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Jwt:Issuer"] = "test-issuer",
                ["Auth:Jwt:Audience"] = "test-audience",
                ["Auth:Jwt:SigningKey"] = "test-signing-key-with-more-than-32-bytes",
                ["Auth:Jwt:AccessTokenMinutes"] = "10",
                ["Auth:Identity:RequireUniqueEmail"] = "true",
                ["Auth:Identity:RequireConfirmedEmail"] = "true",
                ["Auth:Identity:LockoutAllowedForNewUsers"] = "true",
                ["Auth:Identity:MaxFailedAccessAttempts"] = "5"
            })
            .Build();
    }
}

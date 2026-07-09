using Auth.Infrastructure;
using Auth.Infrastructure.Identity;
using Auth.Infrastructure.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Auth.IntegrationTests.Configuration;

public sealed class AuthOptionsValidationTests
{
    [Theory]
    [InlineData("Auth:Jwt:SigningKey", "")]
    [InlineData("Auth:Jwt:SigningKey", "CHANGE_ME_CHANGE_ME_CHANGE_ME_CHANGE")]
    [InlineData("Auth:Jwt:SigningKey", "short")]
    [InlineData("Auth:Jwt:AccessTokenMinutes", "0")]
    [InlineData("Auth:Jwt:AccessTokenMinutes", "1441")]
    public void Invalid_jwt_config_fails_validation(string key, string value)
    {
        using var provider = CreateProvider(new Dictionary<string, string?>
        {
            [key] = value
        });

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<AuthTokenOptions>>().Value);
    }

    [Fact]
    public void Valid_auth_config_resolves_cleanly()
    {
        using var provider = CreateProvider();

        var tokenOptions = provider.GetRequiredService<IOptions<AuthTokenOptions>>().Value;
        var identityOptions = provider.GetRequiredService<IOptions<AuthIdentityOptions>>().Value;

        Assert.Equal("ProjectName", tokenOptions.Issuer);
        Assert.True(identityOptions.RequireUniqueEmail);
    }

    private static ServiceProvider CreateProvider(IReadOnlyDictionary<string, string?>? overrides = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["Auth:Identity:RequireUniqueEmail"] = "true",
            ["Auth:Identity:RequireConfirmedEmail"] = "true",
            ["Auth:Identity:LockoutAllowedForNewUsers"] = "true",
            ["Auth:Identity:MaxFailedAccessAttempts"] = "5",
            ["Auth:Jwt:Issuer"] = "ProjectName",
            ["Auth:Jwt:Audience"] = "ProjectName",
            ["Auth:Jwt:SigningKey"] = "0123456789abcdef0123456789abcdef",
            ["Auth:Jwt:AccessTokenMinutes"] = "10"
        };

        if (overrides is not null)
        {
            foreach ((string key, string? value) in overrides)
            {
                values[key] = value;
            }
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var services = new ServiceCollection();
        services.AddAuthInfra(configuration);

        return services.BuildServiceProvider();
    }
}

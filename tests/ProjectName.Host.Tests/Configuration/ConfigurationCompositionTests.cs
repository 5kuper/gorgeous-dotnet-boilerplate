using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ProjectName.Host.Composition;
using Shared.AppModel.Abstractions;
using Shared.Conventions;

namespace ProjectName.Host.Tests.Configuration;

public sealed class ConfigurationCompositionTests
{
    [Fact]
    public void Loads_root_module_and_feature_json_files()
    {
        using var contentRoot = HostContentRoot.Create();
        contentRoot.Write("config/root/Persistence.config.json", """
            {
              "Persistence": {
                "Provider": "Sqlite",
                "ConnectionString": "Data Source=from-root.db",
                "ApplyMigrationsOnStartup": false
              }
            }
            """);
        contentRoot.Write("config/modules/Auth.config.json", """
            {
              "Auth": {
                "Jwt": {
                  "Issuer": "ModuleIssuer",
                  "Audience": "ModuleAudience",
                  "SigningKey": "",
                  "AccessTokenMinutes": 10
                }
              }
            }
            """);
        contentRoot.Write("config/features/Features.config.json", """
            {
              "FeatureManagement": {
                "PlaceholderScopeA.PlaceholderFeature": true
              }
            }
            """);

        var builder = CreateBuilder(contentRoot.Path);
        builder.AddProjectConfiguration([]);

        Assert.Equal("Data Source=from-root.db", builder.Configuration["Persistence:ConnectionString"]);
        Assert.Equal("ModuleIssuer", builder.Configuration["Auth:Jwt:Issuer"]);
        Assert.Equal("True", builder.Configuration["FeatureManagement:PlaceholderScopeA.PlaceholderFeature"]);
    }

    [Fact]
    public void Environment_specific_config_overrides_base_config()
    {
        using var contentRoot = HostContentRoot.Create();
        contentRoot.Write("config/root/Persistence.config.json", """
            {
              "Persistence": {
                "Provider": "Sqlite",
                "ConnectionString": "Data Source=base.db",
                "ApplyMigrationsOnStartup": false
              }
            }
            """);
        contentRoot.Write("config/root/Persistence.Testing.config.json", """
            {
              "Persistence": {
                "ConnectionString": "Data Source=testing.db"
              }
            }
            """);

        var builder = CreateBuilder(contentRoot.Path, "Testing");
        builder.AddProjectConfiguration([]);

        Assert.Equal("Data Source=testing.db", builder.Configuration["Persistence:ConnectionString"]);
    }

    [Fact]
    public void Command_line_overrides_project_json_files()
    {
        using var contentRoot = HostContentRoot.Create();
        contentRoot.Write("config/root/Persistence.config.json", """
            {
              "Persistence": {
                "Provider": "Sqlite",
                "ConnectionString": "Data Source=base.db",
                "ApplyMigrationsOnStartup": false
              }
            }
            """);
        string[] args = ["--Persistence:ConnectionString=Data Source=command-line.db"];

        var builder = CreateBuilder(contentRoot.Path, args: args);
        builder.AddProjectConfiguration(args);

        Assert.Equal("Data Source=command-line.db", builder.Configuration["Persistence:ConnectionString"]);
    }

    [Fact]
    public void Azure_disabled_does_not_require_endpoint_or_connection()
    {
        using var contentRoot = HostContentRoot.Create();
        contentRoot.Write("config/root/Azure.config.json", """
            {
              "Azure": {
                "AppConfiguration": {
                  "Enabled": false,
                  "Endpoint": ""
                }
              }
            }
            """);

        var builder = CreateBuilder(contentRoot.Path);
        builder.AddProjectConfiguration([]);
        builder.Services.AddConfigurationServices(builder.Configuration);

        using var provider = builder.Services.BuildServiceProvider();

        Assert.Equal("False", builder.Configuration["Azure:AppConfiguration:Enabled"]);
    }

    [Fact]
    public void Azure_disabled_does_not_require_azure_app_configuration_middleware()
    {
        using var contentRoot = HostContentRoot.Create();
        contentRoot.Write("config/root/Azure.config.json", """
            {
              "Azure": {
                "AppConfiguration": {
                  "Enabled": false,
                  "Endpoint": ""
                }
              }
            }
            """);

        var builder = CreateBuilder(contentRoot.Path);
        builder.AddProjectConfiguration([]);
        builder.Services.AddConfigurationServices(builder.Configuration);

        using var app = builder.Build();

        app.UseProjectConfiguration();
    }

    [Fact]
    public async Task Feature_gate_reads_feature_management_flags()
    {
        using var contentRoot = HostContentRoot.Create();
        contentRoot.Write("config/root/Azure.config.json", """
            {
              "Azure": {
                "AppConfiguration": {
                  "Enabled": false,
                  "Endpoint": ""
                }
              }
            }
            """);
        contentRoot.Write("config/features/Features.config.json", """
            {
              "FeatureManagement": {
                "PlaceholderScopeA.PlaceholderFeature": true
              }
            }
            """);

        var builder = CreateBuilder(contentRoot.Path);
        builder.AddProjectConfiguration([]);
        builder.Services.AddConfigurationServices(builder.Configuration);
        using var provider = builder.Services.BuildServiceProvider();

        var gate = provider.GetRequiredService<IFeatureGate>();

        Assert.True(await gate.IsEnabledAsync(AppFeatures.PlaceholderScopeAPlaceholderFeature));
    }

    [Fact]
    public async Task Environment_specific_feature_flags_override_by_feature_name()
    {
        using var contentRoot = HostContentRoot.Create();
        contentRoot.Write("config/root/Azure.config.json", """
            {
              "Azure": {
                "AppConfiguration": {
                  "Enabled": false,
                  "Endpoint": ""
                }
              }
            }
            """);
        contentRoot.Write("config/features/Features.config.json", """
            {
              "FeatureManagement": {
                "PlaceholderScopeA.PlaceholderFeature": false,
                "PlaceholderScopeB.PlaceholderFeature": true
              }
            }
            """);
        contentRoot.Write("config/features/Features.Testing.config.json", """
            {
              "FeatureManagement": {
                "PlaceholderScopeB.PlaceholderFeature": false
              }
            }
            """);

        var builder = CreateBuilder(contentRoot.Path, "Testing");
        builder.AddProjectConfiguration([]);
        builder.Services.AddConfigurationServices(builder.Configuration);
        using var provider = builder.Services.BuildServiceProvider();

        var gate = provider.GetRequiredService<IFeatureGate>();

        Assert.False(await gate.IsEnabledAsync(AppFeatures.PlaceholderScopeAPlaceholderFeature));
        Assert.False(await gate.IsEnabledAsync(AppFeatures.PlaceholderScopeBPlaceholderFeature));
    }

    private static WebApplicationBuilder CreateBuilder(
        string contentRootPath,
        string environmentName = "Production",
        string[]? args = null)
    {
        return WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args ?? [],
            ContentRootPath = contentRootPath,
            EnvironmentName = environmentName,
            ApplicationName = typeof(Program).Assembly.GetName().Name
        });
    }
}

internal sealed class HostContentRoot : IDisposable
{
    private HostContentRoot(string path)
    {
        Path = path;
        Write("appsettings.json", "{}");
    }

    public string Path { get; }

    public static HostContentRoot Create()
    {
        string path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "ProjectName.Host.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(path);

        return new HostContentRoot(path);
    }

    public void Write(string relativePath, string contents)
    {
        string fullPath = System.IO.Path.Combine(
            Path,
            relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        string? directory = System.IO.Path.GetDirectoryName(fullPath);

        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, contents);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}

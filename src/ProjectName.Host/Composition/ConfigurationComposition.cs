using Azure.Identity;
using Gorgeous.Web.Configuration;
using Microsoft.FeatureManagement;
using ProjectName.Host.Features;
using Shared.AppModel.Abstractions;
using HostAzureAppConfigurationOptions = ProjectName.Host.Configuration.AzureAppConfigurationOptions;
using HostAzureAppConfigurationOptionsValidator = ProjectName.Host.Configuration.AzureAppConfigurationOptionsValidator;

namespace ProjectName.Host.Composition;

internal static class ConfigurationComposition
{
    private static readonly string[] ConfigurationGroups = ["root", "modules", "features"];
    private const string ProjectConfigSuffix = ".config.json";

    public static WebApplicationBuilder AddProjectConfiguration(
        this WebApplicationBuilder builder,
        string[] args)
    {
        string environmentName = builder.Environment.EnvironmentName;
        string contentRootPath = builder.Environment.ContentRootPath;

        var bootstrapConfiguration = CreateBootstrapConfiguration(contentRootPath, environmentName, args);
        var azureOptions = BindAzureAppConfigurationOptions(bootstrapConfiguration);

        builder.Configuration.Sources.Clear();
        builder.Configuration
            .SetBasePath(contentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        AddProjectJsonFiles(builder.Configuration, contentRootPath, environmentName, environmentSpecific: false);

        builder.Configuration
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

        AddProjectJsonFiles(builder.Configuration, contentRootPath, environmentName, environmentSpecific: true);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
        }

        builder.Services.AddAzureAppConfiguration();

        if (azureOptions.Enabled && !string.IsNullOrWhiteSpace(azureOptions.Endpoint))
        {
            var credential = new DefaultAzureCredential();

            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options
                    .Connect(new Uri(azureOptions.Endpoint), credential)
                    .ConfigureKeyVault(keyVault => keyVault.SetCredential(credential))
                    .UseFeatureFlags();
            });
        }

        builder.Configuration
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        return builder;
    }

    public static IServiceCollection AddConfigurationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatedOptions<HostAzureAppConfigurationOptions, HostAzureAppConfigurationOptionsValidator>(
            configuration,
            HostAzureAppConfigurationOptions.SectionName);

        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        services.AddScoped<IFeatureGate, MicrosoftFeatureGate>();

        return services;
    }

    public static WebApplication UseProjectConfiguration(this WebApplication app)
    {
        var azureOptions = BindAzureAppConfigurationOptions(app.Configuration);

        if (azureOptions.Enabled && !string.IsNullOrWhiteSpace(azureOptions.Endpoint))
        {
            app.UseAzureAppConfiguration();
        }

        return app;
    }

    private static IConfigurationRoot CreateBootstrapConfiguration(
        string contentRootPath,
        string environmentName,
        string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(contentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        AddProjectJsonFiles(configurationBuilder, contentRootPath, environmentName, environmentSpecific: false);

        configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false);

        AddProjectJsonFiles(configurationBuilder, contentRootPath, environmentName, environmentSpecific: true);

        if (string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
        {
            configurationBuilder.AddUserSecrets<Program>(optional: true, reloadOnChange: false);
        }

        configurationBuilder
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        return configurationBuilder.Build();
    }

    private static HostAzureAppConfigurationOptions BindAzureAppConfigurationOptions(IConfiguration configuration)
    {
        var options = new HostAzureAppConfigurationOptions();

        configuration
            .GetSection(HostAzureAppConfigurationOptions.SectionName)
            .Bind(options);

        return options;
    }

    private static void AddProjectJsonFiles(
        IConfigurationBuilder configuration,
        string contentRootPath,
        string environmentName,
        bool environmentSpecific)
    {
        foreach (string group in ConfigurationGroups)
        {
            string directory = Path.Combine(contentRootPath, "config", group);

            if (!Directory.Exists(directory))
            {
                continue;
            }

            string searchPattern = environmentSpecific
                ? $"*.{environmentName}{ProjectConfigSuffix}"
                : $"*{ProjectConfigSuffix}";

            IEnumerable<string> files = Directory
                .EnumerateFiles(directory, searchPattern, SearchOption.TopDirectoryOnly)
                .Where(file => environmentSpecific || IsBaseProjectConfigFile(file))
                .Order(StringComparer.OrdinalIgnoreCase);

            foreach (string file in files)
            {
                configuration.AddJsonFile(file, optional: true, reloadOnChange: true);
            }
        }
    }

    private static bool IsBaseProjectConfigFile(string file)
    {
        string fileName = Path.GetFileName(file);
        string ownerName = fileName[..^ProjectConfigSuffix.Length];

        return !ownerName.Contains('.', StringComparison.Ordinal);
    }
}

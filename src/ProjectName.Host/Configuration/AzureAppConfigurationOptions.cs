namespace ProjectName.Host.Configuration;

internal sealed class AzureAppConfigurationOptions
{
    public const string SectionName = "Azure:AppConfiguration";

    public bool Enabled { get; init; }

    public string Endpoint { get; init; } = string.Empty;
}

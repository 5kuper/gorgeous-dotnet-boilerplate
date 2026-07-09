using Microsoft.Extensions.Options;

namespace ProjectName.Host.Configuration;

internal sealed class AzureAppConfigurationOptionsValidator : IValidateOptions<AzureAppConfigurationOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureAppConfigurationOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            return ValidateOptionsResult.Fail(
                $"{AzureAppConfigurationOptions.SectionName}:Endpoint is required when Azure App Configuration is enabled.");
        }

        return Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                $"{AzureAppConfigurationOptions.SectionName}:Endpoint must be an absolute URI.");
    }
}

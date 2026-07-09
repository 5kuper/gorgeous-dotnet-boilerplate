using Microsoft.Extensions.Options;

namespace ProjectName.Ai;

internal sealed class AiOptionsValidator : IValidateOptions<AiOptions>
{
    public ValidateOptionsResult Validate(string? name, AiOptions options)
    {
        var failures = new List<string>();

        ValidateProviders(options, failures);
        ValidateScenarios(options, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateProviders(AiOptions options, List<string> failures)
    {
        foreach (var (providerKey, providerOptions) in options.Providers)
        {
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                failures.Add($"{AiOptions.SectionName}:Providers:<key> is required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(providerOptions.Type))
            {
                failures.Add($"{AiOptions.SectionName}:Providers:{providerKey}:Type is required.");
            }
        }
    }

    private static void ValidateScenarios(AiOptions options, List<string> failures)
    {
        foreach (var (scenarioKey, scenarioOptions) in options.Scenarios)
        {
            if (string.IsNullOrWhiteSpace(scenarioKey))
            {
                failures.Add($"{AiOptions.SectionName}:Scenarios:<key> is required.");
                continue;
            }

            bool hasProvider = !string.IsNullOrWhiteSpace(scenarioOptions.Provider);
            bool hasModel = !string.IsNullOrWhiteSpace(scenarioOptions.Model);

            if (hasProvider && !hasModel)
            {
                failures.Add($"{AiOptions.SectionName}:Scenarios:{scenarioKey}:Model is required when Provider is configured.");
            }

            if (hasModel && !hasProvider)
            {
                failures.Add($"{AiOptions.SectionName}:Scenarios:{scenarioKey}:Provider is required when Model is configured.");
            }

            if (hasProvider &&
                hasModel &&
                options.Providers.Count > 0 &&
                !ProviderExists(options, scenarioOptions.Provider!))
            {
                failures.Add($"{AiOptions.SectionName}:Scenarios:{scenarioKey}:Provider must reference a configured provider.");
            }
        }
    }

    private static bool ProviderExists(AiOptions options, string provider)
    {
        foreach (string providerKey in options.Providers.Keys)
        {
            if (string.Equals(providerKey, provider, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}


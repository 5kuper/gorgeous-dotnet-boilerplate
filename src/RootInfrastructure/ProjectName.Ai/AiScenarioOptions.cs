using Gorgeous.Abstractions.Ai.Models;
using Microsoft.Extensions.Configuration;
using Shared.Kernel.Ai;

namespace ProjectName.Ai;

internal sealed class AiScenarioOptions
{
    public const string SectionName = "Ai:Scenarios";

    private readonly IReadOnlyDictionary<string, AiScenarioModelOptions> _scenarios;

    private AiScenarioOptions(IReadOnlyDictionary<string, AiScenarioModelOptions> scenarios)
    {
        _scenarios = scenarios;
    }

    public static AiScenarioOptions FromConfiguration(IConfiguration configuration)
    {
        var scenarios = new Dictionary<string, AiScenarioModelOptions>();

        foreach (var scenarioSection in configuration.GetSection(SectionName).GetChildren())
        {
            scenarios[scenarioSection.Key] = new AiScenarioModelOptions(
                scenarioSection["Provider"],
                scenarioSection["Model"]);
        }

        return new AiScenarioOptions(scenarios);
    }

    public bool TryGetModelSelection(AiScenario scenario, out AiModelSelection? modelSelection)
    {
        modelSelection = null;

        if (!_scenarios.TryGetValue(scenario.Value, out var scenarioOptions))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(scenarioOptions.Provider) ||
            string.IsNullOrWhiteSpace(scenarioOptions.Model))
        {
            return true;
        }

        modelSelection = new AiModelSelection(
            new AiProviderKey(scenarioOptions.Provider),
            new AiModelKey(scenarioOptions.Model));

        return true;
    }
}

internal sealed record AiScenarioModelOptions(string? Provider, string? Model);

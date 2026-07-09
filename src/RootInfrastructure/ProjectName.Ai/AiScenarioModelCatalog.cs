using Gorgeous.Abstractions.Ai.Models;
using Microsoft.Extensions.Options;
using Shared.Kernel.Ai;

namespace ProjectName.Ai;

internal sealed class AiScenarioModelCatalog
{
    private readonly IOptions<AiOptions> _options;

    public AiScenarioModelCatalog(IOptions<AiOptions> options)
    {
        _options = options;
    }

    public bool TryGetModelSelection(AiScenario scenario, out AiModelSelection? modelSelection)
    {
        modelSelection = null;

        if (!_options.Value.Scenarios.TryGetValue(scenario.Value, out var scenarioOptions))
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

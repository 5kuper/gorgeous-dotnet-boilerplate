using Gorgeous.Abstractions.Ai.Models;
using Gorgeous.Abstractions.Results;
using Shared.AppModel.Ai;
using Shared.Kernel.Ai;

namespace ProjectName.Ai;

internal sealed class AiScenarioModelResolver(AiScenarioOptions options) : IAiScenarioModelResolver
{
    public Result<AiModelSelection> Resolve(AiScenario scenario)
    {
        if (scenario.IsEmpty)
        {
            return Result.Failure<AiModelSelection>(
                new Error("Ai.ScenarioMissing", "AI scenario is required.", ErrorType.Validation));
        }

        if (!options.TryGetModelSelection(scenario, out var modelSelection))
        {
            return Result.Failure<AiModelSelection>(
                new Error("Ai.ScenarioUnknown", $"AI scenario '{scenario}' is not configured.", ErrorType.NotFound));
        }

        return modelSelection is null
            ? Result.Failure<AiModelSelection>(
                new Error("Ai.ScenarioModelMissing", $"AI scenario '{scenario}' must configure Provider and Model.", ErrorType.Validation))
            : Result.Success(modelSelection);
    }
}

using Gorgeous.Abstractions.Ai.Models;
using Gorgeous.Abstractions.Results;
using Shared.Kernel.Ai;

namespace Shared.AppModel.Ai;

public interface IAiScenarioModelResolver
{
    Result<AiModelSelection> Resolve(AiScenario scenario);
}

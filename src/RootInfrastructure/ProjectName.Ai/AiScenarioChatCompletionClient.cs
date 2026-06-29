using Gorgeous.Abstractions.Ai.Chat;
using Gorgeous.Abstractions.Ai.Generation;
using Gorgeous.Abstractions.Ai.Models;
using Gorgeous.Abstractions.Results;
using Gorgeous.Ai;
using Shared.AppModel.Ai;
using Shared.Kernel.Ai;

namespace ProjectName.Ai;

internal sealed class AiScenarioChatCompletionClient(
    IAiScenarioModelResolver modelResolver,
    IAiChatCompletionClientResolver clientResolver) : IAiScenarioChatCompletionClient
{
    public async Task<Result<AiChatCompletionResponse>> CompleteAsync(
        AiScenario scenario,
        IReadOnlyList<AiChatMessage> messages,
        AiGenerationOptions? options = null,
        CancellationToken ct = default)
    {
        var modelSelectionResult = modelResolver.Resolve(scenario);

        if (modelSelectionResult.IsFailure)
        {
            return Result.Failure<AiChatCompletionResponse>(modelSelectionResult.Error);
        }

        var clientResult =
            clientResolver.Resolve(modelSelectionResult.Value.Provider);

        if (clientResult.IsFailure)
        {
            return Result.Failure<AiChatCompletionResponse>(clientResult.Error);
        }

        var request = new AiChatCompletionRequest(modelSelectionResult.Value, messages, options);

        return await clientResult.Value.CompleteAsync(request, ct);
    }
}

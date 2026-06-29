using Gorgeous.Abstractions.Ai.Chat;
using Gorgeous.Abstractions.Ai.Generation;
using Gorgeous.Abstractions.Results;
using Shared.Kernel.Ai;

namespace Shared.AppModel.Ai;

public interface IAiScenarioChatCompletionClient
{
    Task<Result<AiChatCompletionResponse>> CompleteAsync(
        AiScenario scenario,
        IReadOnlyList<AiChatMessage> messages,
        AiGenerationOptions? options = null,
        CancellationToken ct = default);
}

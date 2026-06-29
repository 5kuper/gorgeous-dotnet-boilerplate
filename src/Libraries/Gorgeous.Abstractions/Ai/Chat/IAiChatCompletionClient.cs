using Gorgeous.Abstractions.Results;

namespace Gorgeous.Abstractions.Ai.Chat;

public interface IAiChatCompletionClient
{
    Task<Result<AiChatCompletionResponse>> CompleteAsync(
        AiChatCompletionRequest request,
        CancellationToken ct = default);
}

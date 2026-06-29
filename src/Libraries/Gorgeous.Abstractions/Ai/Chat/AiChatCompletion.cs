using Gorgeous.Abstractions.Ai.Generation;
using Gorgeous.Abstractions.Ai.Models;

namespace Gorgeous.Abstractions.Ai.Chat;

public sealed record AiChatCompletionRequest(
    AiModelSelection Model,
    IReadOnlyList<AiChatMessage> Messages,
    AiGenerationOptions? Options = null);

public sealed record AiChatCompletionResponse(
    AiModelSelection Model,
    AiChatMessage Message,
    string? FinishReason = null);

public sealed record AiChatMessage(
    AiChatRole Role,
    string Content,
    string? Name = null);

public enum AiChatRole
{
    System = 0,
    User = 1,
    Assistant = 2,
    Tool = 3,
}

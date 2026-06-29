using Gorgeous.Abstractions.Ai.Chat;
using Gorgeous.Abstractions.Ai.Models;
using Gorgeous.Abstractions.Results;

namespace Gorgeous.Ai;

public interface IAiChatCompletionClientResolver
{
    Result<IAiChatCompletionClient> Resolve(AiProviderKey providerKey);
}

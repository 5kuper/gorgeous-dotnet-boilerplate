using Gorgeous.Abstractions.Ai.Chat;
using Gorgeous.Abstractions.Ai.Models;
using Gorgeous.Abstractions.Results;

namespace Gorgeous.Ai;

internal sealed class AiChatCompletionClientResolver(
    IEnumerable<AiChatCompletionClientDescriptor> descriptors) : IAiChatCompletionClientResolver
{
    public Result<IAiChatCompletionClient> Resolve(AiProviderKey providerKey)
    {
        if (providerKey.IsEmpty)
        {
            return Result.Failure<IAiChatCompletionClient>(
                new Error("Ai.ProviderKeyMissing", "AI provider key is required.", ErrorType.Validation));
        }

        var client = descriptors
            .Where(descriptor => descriptor.ProviderKey == providerKey)
            .Select(descriptor => descriptor.Client)
            .SingleOrDefault();

        return client is null
            ? Result.Failure<IAiChatCompletionClient>(
                new Error("Ai.ProviderNotRegistered", $"AI provider '{providerKey}' is not registered.", ErrorType.NotFound))
            : Result.Success(client);
    }
}

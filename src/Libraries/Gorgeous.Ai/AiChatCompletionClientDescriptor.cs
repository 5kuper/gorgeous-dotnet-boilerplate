using Gorgeous.Abstractions.Ai.Chat;
using Gorgeous.Abstractions.Ai.Models;

namespace Gorgeous.Ai;

public sealed record AiChatCompletionClientDescriptor(
    AiProviderKey ProviderKey,
    IAiChatCompletionClient Client);

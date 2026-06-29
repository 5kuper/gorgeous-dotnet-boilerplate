using Gorgeous.Abstractions.Ai.Chat;
using Gorgeous.Abstractions.Ai.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gorgeous.Ai;

public static class GorgeousAiRegistration
{
    public static IServiceCollection AddGorgeousAi(this IServiceCollection services)
    {
        services.TryAddScoped<IAiChatCompletionClientResolver, AiChatCompletionClientResolver>();

        return services;
    }

    public static IServiceCollection AddAiChatCompletionClient<TClient>(
        this IServiceCollection services,
        AiProviderKey providerKey)
        where TClient : class, IAiChatCompletionClient
    {
        services.AddScoped<TClient>();
        services.AddScoped(serviceProvider => new AiChatCompletionClientDescriptor(
            providerKey,
            serviceProvider.GetRequiredService<TClient>()));

        return services;
    }
}

using Gorgeous.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.AppModel.Ai;

namespace ProjectName.Ai;

public static class ProjectNameAiRegistration
{
    public static IServiceCollection AddProjectNameAi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddGorgeousAi();

        services.AddSingleton(AiScenarioOptions.FromConfiguration(configuration));
        services.AddScoped<IAiScenarioModelResolver, AiScenarioModelResolver>();
        services.AddScoped<IAiScenarioChatCompletionClient, AiScenarioChatCompletionClient>();

        return services;
    }
}

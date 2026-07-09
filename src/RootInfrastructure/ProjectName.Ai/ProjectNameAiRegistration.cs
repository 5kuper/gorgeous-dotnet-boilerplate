using Gorgeous.Ai;
using Gorgeous.Web.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.AppModel.Ai;

namespace ProjectName.Ai;

public static class ProjectNameAiRegistration
{
    public static IServiceCollection AddProjectNameAi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddGorgeousAi();

        services.AddValidatedOptions<AiOptions, AiOptionsValidator>(
            configuration,
            AiOptions.SectionName);

        services.AddSingleton(serviceProvider =>
            new AiScenarioModelCatalog(serviceProvider.GetRequiredService<IOptions<AiOptions>>()));

        services.AddScoped<IAiScenarioModelResolver, AiScenarioModelResolver>();
        services.AddScoped<IAiScenarioChatCompletionClient, AiScenarioChatCompletionClient>();

        return services;
    }
}

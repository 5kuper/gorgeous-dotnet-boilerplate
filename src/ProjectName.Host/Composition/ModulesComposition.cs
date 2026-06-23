using Auth.Application;
using Auth.Infrastructure;
using ProjectName.Persistence;
using Users.Application;
using Users.Infrastructure;

namespace ProjectName.Host.Composition;

internal static class ModulesComposition
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddUsersCore();
        services.AddUsersInfra();

        services.AddAuthCore();
        services.AddAuthInfra(configuration);

        services.AddProjectNamePersistence(configuration);

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Users.Infrastructure;
using Users.Infrastructure.Persistence;

namespace Users.IntegrationTests.Persistence.Support;

internal static class UsersPersistenceServices
{
    public static ServiceProvider CreateServiceProvider(UsersDbContext context)
    {
        var services = new ServiceCollection();
        services.AddUsersInfra();
        services.AddSingleton(context);

        return services.BuildServiceProvider();
    }
}

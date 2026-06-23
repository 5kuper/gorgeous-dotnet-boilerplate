using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application;

public static class AuthCoreRegistration
{
    public static IServiceCollection AddAuthCore(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(AuthCoreRegistration).Assembly));

        return services;
    }
}

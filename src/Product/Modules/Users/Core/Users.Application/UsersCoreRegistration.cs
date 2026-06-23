using Microsoft.Extensions.DependencyInjection;
using Users.Application.Features.EmailVerification;
using Users.Application.Features.Registration;
using Users.Contracts.Registration;
using Users.Contracts.Verification;

namespace Users.Application;

public static class UsersCoreRegistration
{
    public static IServiceCollection AddUsersCore(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(UsersCoreRegistration).Assembly));

        services.AddScoped<IUsersRegistration, UsersRegistrationService>();
        services.AddScoped<IUsersEmailVerification, UsersEmailConfirmationService>();

        return services;
    }
}

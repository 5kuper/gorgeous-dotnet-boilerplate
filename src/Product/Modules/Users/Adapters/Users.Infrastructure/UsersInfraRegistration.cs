using Microsoft.Extensions.DependencyInjection;
using Shared.AppModel.Abstractions;
using Users.Contracts.Authentication;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Persistence.Readers;
using Users.Infrastructure.Persistence.Repositories;

namespace Users.Infrastructure;

public static class UsersInfraRegistration
{
    public static IServiceCollection AddUsersInfra(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<UsersDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserAuthProfileReader, EfUserAuthProfileReader>();

        return services;
    }
}

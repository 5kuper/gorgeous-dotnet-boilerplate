using System.Data.Common;
using Auth.Application.Ports.Registration;
using Auth.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectName.Persistence.Transactions;
using Users.Infrastructure.Persistence;

namespace ProjectName.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddProjectNamePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=projectname.db";

        services.AddScoped<DbConnection>(_ => new SqliteConnection(connectionString));

        services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
            options.UseSqlite(
                serviceProvider.GetRequiredService<DbConnection>(),
                sqlite => sqlite.MigrationsHistoryTable("__AuthMigrationsHistory")));

        services.AddDbContext<UsersDbContext>((serviceProvider, options) =>
            options.UseSqlite(
                serviceProvider.GetRequiredService<DbConnection>(),
                sqlite => sqlite.MigrationsHistoryTable("__UsersMigrationsHistory")));

        services.AddScoped<IRegistrationTransaction, EfRegistrationTransaction>();
        services.AddScoped<EfSharedTransactionRunner>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}

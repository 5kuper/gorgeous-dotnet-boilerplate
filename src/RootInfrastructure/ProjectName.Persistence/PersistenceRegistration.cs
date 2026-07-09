using System.Data.Common;
using Auth.Application.Ports.Registration;
using Auth.Infrastructure.Persistence;
using Gorgeous.Web.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectName.Persistence.Transactions;
using Users.Infrastructure.Persistence;

namespace ProjectName.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddProjectNamePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatedOptions<PersistenceOptions, PersistenceOptionsValidator>(
            configuration,
            PersistenceOptions.SectionName);

        services.AddScoped<DbConnection>(serviceProvider =>
            new SqliteConnection(
                serviceProvider.GetRequiredService<IOptions<PersistenceOptions>>().Value.ConnectionString));

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

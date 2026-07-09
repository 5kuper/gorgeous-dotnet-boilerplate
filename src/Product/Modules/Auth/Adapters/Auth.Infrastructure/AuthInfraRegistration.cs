using Auth.Application.Ports.Identity;
using Auth.Application.Ports.Messaging;
using Auth.Application.Ports.Tokens;
using Auth.Application.Ports.Verification;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Identity;
using Auth.Infrastructure.Messaging;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Persistence.Repositories;
using Auth.Infrastructure.Tokens;
using Auth.Infrastructure.Verification;
using Gorgeous.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Auth.Infrastructure;

public static class AuthInfraRegistration
{
    public static IServiceCollection AddAuthInfra(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddIdentityCore<AppIdentityUser>()
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddValidatedOptions<AuthIdentityOptions, AuthIdentityOptionsValidator>(
            configuration,
            AuthIdentityOptions.SectionName);

        services
            .AddOptions<IdentityOptions>()
            .Configure<IOptions<AuthIdentityOptions>>((options, authIdentityOptions) =>
            {
                var identityOptions = authIdentityOptions.Value;

                options.User.RequireUniqueEmail = identityOptions.RequireUniqueEmail;
                options.SignIn.RequireConfirmedEmail = identityOptions.RequireConfirmedEmail;
                options.Lockout.AllowedForNewUsers = identityOptions.LockoutAllowedForNewUsers;
                options.Lockout.MaxFailedAccessAttempts = identityOptions.MaxFailedAccessAttempts;
            });

        services.AddDataProtection();
        services.AddValidatedOptions<AuthTokenOptions, AuthTokenOptionsValidator>(
            configuration,
            AuthTokenOptions.SectionName);

        services.AddScoped<IIdentityCredentialService, IdentityCredentialService>();
        services.AddScoped<IRefreshSessionRepository, RefreshSessionRepository>();
        services.AddScoped<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IEmailConfirmationCodeProtector, DataProtectionEmailConfirmationCodeProtector>();
        services.AddScoped<IEmailConfirmationSender, NoOpEmailConfirmationSender>();
        services.AddScoped<IPasswordResetSender, NoOpPasswordResetSender>();

        return services;
    }
}

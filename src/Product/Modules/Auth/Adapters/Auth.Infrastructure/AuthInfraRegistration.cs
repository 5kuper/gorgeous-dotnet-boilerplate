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
using Microsoft.AspNetCore.DataProtection;
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
            .AddIdentityCore<AppIdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddDataProtection();
        services
            .AddOptions<AuthTokenOptions>()
            .Bind(configuration.GetSection(AuthTokenOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AuthTokenOptions>, AuthTokenOptionsValidator>();

        services.AddScoped<IIdentityCredentialService, IdentityCredentialService>();
        services.AddScoped<IRefreshSessionRepository, RefreshSessionRepository>();
        services.AddScoped<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IEmailConfirmationCodeProtector, DataProtectionEmailConfirmationCodeProtector>();
        services.AddScoped<IEmailConfirmationSender, NoOpEmailConfirmationSender>();
        services.AddScoped<IPasswordResetSender, NoOpPasswordResetSender>();

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gorgeous.Web.Configuration;

public static class ValidatedOptionsServiceCollectionExtensions
{
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        services.AddSingleton<IValidateOptions<TOptions>, TValidator>();

        return services
            .AddOptions<TOptions>()
            .Bind(configuration.GetRequiredSection(sectionName))
            .ValidateOnStart();
    }
}

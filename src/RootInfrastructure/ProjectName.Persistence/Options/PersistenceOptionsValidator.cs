using Microsoft.Extensions.Options;

namespace ProjectName.Persistence;

internal sealed class PersistenceOptionsValidator : IValidateOptions<PersistenceOptions>
{
    public ValidateOptionsResult Validate(string? name, PersistenceOptions options)
    {
        var failures = new List<string>();

        if (!string.Equals(options.Provider, PersistenceOptions.SqliteProvider, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"{PersistenceOptions.SectionName}:Provider must be '{PersistenceOptions.SqliteProvider}'.");
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add($"{PersistenceOptions.SectionName}:ConnectionString is required.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}


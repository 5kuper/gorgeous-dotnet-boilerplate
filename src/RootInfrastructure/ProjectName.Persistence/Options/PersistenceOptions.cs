namespace ProjectName.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public const string SqliteProvider = "Sqlite";

    public string Provider { get; init; } = SqliteProvider;

    public string ConnectionString { get; init; } = string.Empty;

    public bool ApplyMigrationsOnStartup { get; init; }
}


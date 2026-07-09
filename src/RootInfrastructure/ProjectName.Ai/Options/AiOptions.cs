namespace ProjectName.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public Dictionary<string, AiProviderOptions> Providers { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, AiScenarioModelOptions> Scenarios { get; init; } = new(StringComparer.Ordinal);
}

public sealed class AiProviderOptions
{
    public string Type { get; init; } = string.Empty;
}

public sealed class AiScenarioModelOptions
{
    public string? Provider { get; init; }

    public string? Model { get; init; }
}


namespace Gorgeous.Abstractions.Ai.Generation;

public sealed record AiGenerationOptions(
    int? MaxOutputTokens = null,
    double? Temperature = null,
    double? TopP = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

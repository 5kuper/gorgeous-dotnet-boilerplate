namespace Gorgeous.Abstractions.Ai.Models;

public sealed record AiProviderKey(string Value)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    public override string ToString() => Value;
}

public sealed record AiModelKey(string Value)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    public override string ToString() => Value;
}

public sealed record AiModelSelection(AiProviderKey Provider, AiModelKey Model);

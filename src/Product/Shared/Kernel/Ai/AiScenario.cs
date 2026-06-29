namespace Shared.Kernel.Ai;

public sealed record AiScenario(string Value)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    public override string ToString() => Value;
}

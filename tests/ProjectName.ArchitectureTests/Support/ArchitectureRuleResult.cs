using System.Text;

namespace ProjectName.ArchitectureTests.Support;

internal sealed class ArchitectureRuleResult
{
    private readonly List<ArchitectureRuleViolation> _violations = [];

    public bool IsValid => _violations.Count == 0;

    public IReadOnlyCollection<ArchitectureRuleViolation> Violations => _violations;

    public void Add(string rule, string offender, string dependency, string expected)
    {
        _violations.Add(new ArchitectureRuleViolation(rule, offender, dependency, expected));
    }

    public override string ToString()
    {
        if (IsValid)
        {
            return "Architecture rule passed.";
        }

        var message = new StringBuilder();
        message.AppendLine("Architecture rule violations:");

        foreach (var violation in _violations)
        {
            message.AppendLine(CultureInvariant($"- Rule: {violation.Rule}"));
            message.AppendLine(CultureInvariant($"  Offender: {violation.Offender}"));
            message.AppendLine(CultureInvariant($"  Dependency: {violation.Dependency}"));
            message.AppendLine(CultureInvariant($"  Expected: {violation.Expected}"));
        }

        return message.ToString();
    }

    private static string CultureInvariant(FormattableString value)
    {
        return FormattableString.Invariant(value);
    }
}

internal sealed record ArchitectureRuleViolation(
    string Rule,
    string Offender,
    string Dependency,
    string Expected);

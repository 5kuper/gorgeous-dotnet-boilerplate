using System.Reflection;
using NetArchTest.Rules;

namespace ProjectName.ArchitectureTests.Support;

internal static class NetArchTestRule
{
    public static ArchitectureRuleResult AssembliesShouldNotDependOn(
        IEnumerable<Assembly> assemblies,
        string rule,
        IReadOnlyCollection<string> forbiddenNamespaces,
        string expected)
    {
        var result = new ArchitectureRuleResult();

        foreach (var assembly in assemblies)
        {
            AssemblyShouldNotDependOn(result, assembly, rule, forbiddenNamespaces, expected);
        }

        return result;
    }

    public static ArchitectureRuleResult AssemblyShouldNotDependOn(
        Assembly assembly,
        string rule,
        IReadOnlyCollection<string> forbiddenNamespaces,
        string expected)
    {
        var result = new ArchitectureRuleResult();
        AssemblyShouldNotDependOn(result, assembly, rule, forbiddenNamespaces, expected);

        return result;
    }

    private static void AssemblyShouldNotDependOn(
        ArchitectureRuleResult result,
        Assembly assembly,
        string rule,
        IReadOnlyCollection<string> forbiddenNamespaces,
        string expected)
    {
        foreach (string forbiddenNamespace in forbiddenNamespaces)
        {
            var testResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn(forbiddenNamespace)
                .GetResult();

            if (testResult.IsSuccessful)
            {
                continue;
            }

            result.Add(
                rule,
                FormatFailingTypes(testResult),
                forbiddenNamespace,
                expected);
        }
    }

    private static string FormatFailingTypes(object testResult)
    {
        string[] failingTypeNames = ReadFailingTypeNames(testResult).ToArray();

        return failingTypeNames.Length == 0
            ? "Unknown failing type"
            : string.Join(", ", failingTypeNames.Order(StringComparer.Ordinal));
    }

    private static IEnumerable<string> ReadFailingTypeNames(object testResult)
    {
        var resultType = testResult.GetType();
        var failingTypeNamesProperty = resultType.GetProperty("FailingTypeNames");

        if (failingTypeNamesProperty?.GetValue(testResult) is IEnumerable<string> failingTypeNames)
        {
            return failingTypeNames;
        }

        var failingTypesProperty = resultType.GetProperty("FailingTypes");

        if (failingTypesProperty?.GetValue(testResult) is IEnumerable<Type> failingTypes)
        {
            return failingTypes.Select(type => type.FullName ?? type.Name);
        }

        return [];
    }
}

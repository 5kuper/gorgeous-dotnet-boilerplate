using ProjectName.ArchitectureTests.Support;

namespace ProjectName.ArchitectureTests;

public sealed class ModuleBoundaryTests
{
    [Fact]
    public void Modules_should_only_depend_on_other_modules_through_contracts()
    {
        var result = new ArchitectureRuleResult();

        foreach (var module in ArchitectureAssemblies.Modules)
        {
            string[] otherModuleNames = ArchitectureAssemblies.Modules
                .Where(otherModule => otherModule.Name != module.Name)
                .Select(otherModule => otherModule.Name)
                .ToArray();

            string[] forbiddenNamespaces = otherModuleNames
                .SelectMany(NonContractNamespaces)
                .ToArray();

            Merge(
                result,
                NetArchTestRule.AssembliesShouldNotDependOn(
                    module.Assemblies,
                    "Module boundary",
                    forbiddenNamespaces,
                    "Modules may reference another module through its Contracts namespace only."));
        }

        result.ShouldBeValid();
    }

    private static IEnumerable<string> NonContractNamespaces(string moduleName)
    {
        yield return $"{moduleName}.Domain";
        yield return $"{moduleName}.Application";
        yield return $"{moduleName}.Infrastructure";
        yield return $"{moduleName}.Presentation";
    }

    private static void Merge(ArchitectureRuleResult target, ArchitectureRuleResult source)
    {
        foreach (var violation in source.Violations)
        {
            target.Add(violation.Rule, violation.Offender, violation.Dependency, violation.Expected);
        }
    }
}

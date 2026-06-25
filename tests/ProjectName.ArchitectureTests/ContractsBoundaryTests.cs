using ProjectName.ArchitectureTests.Support;

namespace ProjectName.ArchitectureTests;

public sealed class ContractsBoundaryTests
{
    private static readonly string[] ForbiddenPlatformOrProviderDependencies =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore.Identity",
        "Microsoft.IdentityModel",
        "System.IdentityModel",
        "Stripe",
    ];

    private static readonly string[] ForbiddenAdapterNamespaceSuffixes =
    [
        "Infrastructure",
        "Presentation",
    ];

    [Fact]
    public void Contracts_should_not_depend_on_transport_persistence_identity_or_provider_types()
    {
        var contractAssemblies = ArchitectureAssemblies.Modules.Select(module => module.Contracts);

        var result = NetArchTestRule.AssembliesShouldNotDependOn(
            contractAssemblies,
            "Contracts boundary",
            ForbiddenPlatformOrProviderDependencies,
            "Contracts must expose stable module APIs without HTTP, EF Core, Identity, or provider-specific types.");

        result.ShouldBeValid();
    }

    [Fact]
    public void Contracts_should_not_depend_on_module_infrastructure_or_presentation()
    {
        var result = new ArchitectureRuleResult();
        string[] forbiddenAdapterNamespaces = ForbiddenAdapterNamespaces();

        foreach (var module in ArchitectureAssemblies.Modules)
        {
            Merge(
                result,
                NetArchTestRule.AssemblyShouldNotDependOn(
                    module.Contracts,
                    "Contracts boundary",
                    forbiddenAdapterNamespaces,
                    "Contracts must not reference any module Infrastructure or Presentation namespaces."));
        }

        result.ShouldBeValid();
    }

    private static string[] ForbiddenAdapterNamespaces()
    {
        return ArchitectureAssemblies.Modules
            .SelectMany(module => ForbiddenAdapterNamespaceSuffixes
                .Select(suffix => $"{module.Name}.{suffix}"))
            .ToArray();
    }

    private static void Merge(ArchitectureRuleResult target, ArchitectureRuleResult source)
    {
        foreach (var violation in source.Violations)
        {
            target.Add(violation.Rule, violation.Offender, violation.Dependency, violation.Expected);
        }
    }
}

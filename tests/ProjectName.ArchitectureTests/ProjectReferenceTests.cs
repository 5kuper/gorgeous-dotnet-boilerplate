using ProjectName.ArchitectureTests.ProjectModel;
using ProjectName.ArchitectureTests.Support;
using System.Xml.Linq;

namespace ProjectName.ArchitectureTests;

public sealed class ProjectReferenceTests
{
    [Fact]
    public void Modules_should_follow_the_expected_project_layout()
    {
        var graph = ArchitectureProjectGraph.LoadFromRepository();

        var result = graph.ValidateModuleLayout();

        result.ShouldBeValid();
    }

    [Fact]
    public void Assembly_checks_should_load_every_discovered_module()
    {
        var graph = ArchitectureProjectGraph.LoadFromRepository();
        string[] discoveredModules = graph.Modules
            .Select(module => module.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] mappedModules = ArchitectureAssemblies.Modules
            .Select(module => module.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            discoveredModules.SequenceEqual(mappedModules, StringComparer.Ordinal),
            "Every discovered module must be loadable through ArchitectureAssemblies.Modules so assembly-level architecture rules cover it." +
            Environment.NewLine +
            $"Discovered: {string.Join(", ", discoveredModules)}" +
            Environment.NewLine +
            $"Loadable: {string.Join(", ", mappedModules)}");
    }

    [Fact]
    public void Projects_should_follow_the_allowed_reference_matrix()
    {
        var graph = ArchitectureProjectGraph.LoadFromRepository();

        var result = AllowedProjectReferences.Validate(graph);

        result.ShouldBeValid();
    }

    [Fact]
    public void Application_projects_should_not_reference_feature_management_or_azure_packages()
    {
        var graph = ArchitectureProjectGraph.LoadFromRepository();
        string[] forbiddenPackagePrefixes = ["Microsoft.FeatureManagement", "Microsoft.Azure", "Azure."];

        string[] offenders = graph.Projects
            .Where(project => project.Layer == ArchitectureLayer.Application)
            .SelectMany(project => ReadPackageReferences(project.Path).Select(package => $"{project.Name} -> {package}"))
            .Where(reference => forbiddenPackagePrefixes.Any(prefix => reference.Contains(prefix, StringComparison.OrdinalIgnoreCase)))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            "Application projects must use Shared.AppModel.Abstractions for feature flags and must not reference Azure SDKs." +
            Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void Azure_and_feature_management_packages_should_stay_in_host_composition()
    {
        var graph = ArchitectureProjectGraph.LoadFromRepository();
        string[] guardedPackages =
        [
            "Azure.Identity",
            "Microsoft.Azure.AppConfiguration.AspNetCore",
            "Microsoft.FeatureManagement.AspNetCore",
        ];

        string[] offenders = graph.Projects
            .Where(project => project.Layer != ArchitectureLayer.Host)
            .SelectMany(project => ReadPackageReferences(project.Path).Select(package => $"{project.Name} -> {package}"))
            .Where(reference => guardedPackages.Any(package => reference.EndsWith(package, StringComparison.OrdinalIgnoreCase)))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            "Azure App Configuration and Microsoft feature-management packages belong in Host/root composition, not modules or shared abstractions." +
            Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    private static IReadOnlyList<string> ReadPackageReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);

        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(package => !string.IsNullOrWhiteSpace(package))
            .Select(package => package!)
            .ToArray();
    }
}

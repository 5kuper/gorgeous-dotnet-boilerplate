using ProjectName.ArchitectureTests.ProjectModel;
using ProjectName.ArchitectureTests.Support;

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
}

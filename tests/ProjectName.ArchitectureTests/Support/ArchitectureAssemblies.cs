using System.Reflection;
using ProjectName.ArchitectureTests.ProjectModel;

namespace ProjectName.ArchitectureTests.Support;

internal static class ArchitectureAssemblies
{
    private static readonly Lazy<ArchitectureProjectGraph> ProjectGraph =
        new(() => ArchitectureProjectGraph.LoadFromRepository());

    private static readonly Lazy<IReadOnlyList<ModuleAssemblySet>> DiscoveredModules =
        new(LoadModuleAssemblySets);

    public static IReadOnlyList<ModuleAssemblySet> Modules => DiscoveredModules.Value;

    public static Assembly Host => LoadAssembly("ProjectName.Host");

    public static Assembly Persistence => LoadAssembly("ProjectName.Persistence");

    public static Assembly SharedCore => LoadAssembly("Shared.BuildingBlocks.Core");

    public static Assembly SharedApplication => LoadAssembly("Shared.BuildingBlocks.Application");

    public static Assembly SharedWebFramework => LoadAssembly("Shared.WebFramework");

    private static IReadOnlyList<ModuleAssemblySet> LoadModuleAssemblySets()
    {
        return ProjectGraph.Value.Modules
            .Select(module => new ModuleAssemblySet(
                module.Name,
                LoadAssembly(module, module.Contracts, "Contracts"),
                LoadAssembly(module, module.Domain, "Domain"),
                LoadAssembly(module, module.Application, "Application"),
                LoadAssembly(module, module.Infrastructure, "Infrastructure"),
                LoadAssembly(module, module.Presentation, "Presentation")))
            .ToArray();
    }

    private static Assembly LoadAssembly(
        ArchitectureModule module,
        ArchitectureProject? project,
        string layerName)
    {
        if (project is null)
        {
            throw new InvalidOperationException(
                $"Cannot load {layerName} assembly for module '{module.Name}' because the expected project is missing.");
        }

        return LoadAssembly(project.Name);
    }

    private static Assembly LoadAssembly(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            throw new InvalidOperationException(
                $"Could not load assembly '{assemblyName}'. Ensure the project is referenced by ProjectName.ArchitectureTests so assembly-level architecture rules can inspect it.",
                ex);
        }
    }
}

internal sealed record ModuleAssemblySet(
    string Name,
    Assembly Contracts,
    Assembly Domain,
    Assembly Application,
    Assembly Infrastructure,
    Assembly Presentation)
{
    public IReadOnlyList<Assembly> Assemblies { get; } =
    [
        Contracts,
        Domain,
        Application,
        Infrastructure,
        Presentation,
    ];
}

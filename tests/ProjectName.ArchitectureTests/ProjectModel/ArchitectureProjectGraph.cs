using System.Runtime.CompilerServices;
using System.Xml.Linq;
using ProjectName.ArchitectureTests.Support;

namespace ProjectName.ArchitectureTests.ProjectModel;

internal sealed class ArchitectureProjectGraph
{
    private readonly Dictionary<string, ArchitectureProject> _projectsByPath;

    private ArchitectureProjectGraph(
        string repositoryRoot,
        IReadOnlyList<ArchitectureProject> projects,
        IReadOnlyList<ArchitectureModule> modules)
    {
        RepositoryRoot = repositoryRoot;
        Projects = projects;
        Modules = modules;
        _projectsByPath = projects.ToDictionary(project => project.Path, StringComparer.OrdinalIgnoreCase);
    }

    public string RepositoryRoot { get; }

    public IReadOnlyList<ArchitectureProject> Projects { get; }

    public IReadOnlyList<ArchitectureModule> Modules { get; }

    public static ArchitectureProjectGraph LoadFromRepository([CallerFilePath] string callerFilePath = "")
    {
        string repositoryRoot = FindRepositoryRoot(callerFilePath);
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        string[] projectPaths = Directory
            .EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(NormalizePath)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var projects = projectPaths
            .Select(path => CreateProject(repositoryRoot, path))
            .ToArray();

        var modules = DiscoverModules(repositoryRoot, projects);

        return new ArchitectureProjectGraph(repositoryRoot, projects, modules);
    }

    public IReadOnlyList<ArchitectureProject> GetReferences(ArchitectureProject project)
    {
        return project.ReferencePaths
            .Select(FindProject)
            .OfType<ArchitectureProject>()
            .ToArray();
    }

    public IReadOnlyList<string> GetUnknownReferencePaths(ArchitectureProject project)
    {
        return project.ReferencePaths
            .Where(referencePath => FindProject(referencePath) is null)
            .ToArray();
    }

    public ArchitectureRuleResult ValidateModuleLayout()
    {
        ArchitectureRuleResult result = new();

        foreach (var module in Modules)
        {
            foreach (string missingProject in module.MissingProjects)
            {
                result.Add(
                    "Module layout",
                    module.Name,
                    missingProject,
                    "Each module must contain Contracts, Domain, Application, Infrastructure, and Presentation projects.");
            }
        }

        return result;
    }

    private static ArchitectureProject CreateProject(string repositoryRoot, string projectPath)
    {
        string projectName = Path.GetFileNameWithoutExtension(projectPath);
        var (layer, moduleName) = ClassifyProject(repositoryRoot, projectPath, projectName);

        return new ArchitectureProject(
            projectName,
            projectPath,
            layer,
            moduleName,
            ReadProjectReferences(projectPath));
    }

    private static (ArchitectureLayer Layer, string? ModuleName) ClassifyProject(
        string repositoryRoot,
        string projectPath,
        string projectName)
    {
        return projectName switch
        {
            "ProjectName.Host" => (ArchitectureLayer.Host, null),
            "ProjectName.Persistence" => (ArchitectureLayer.Persistence, null),
            "Shared.BuildingBlocks.Core" => (ArchitectureLayer.SharedCore, null),
            "Shared.BuildingBlocks.Application" => (ArchitectureLayer.SharedApplication, null),
            "Shared.WebFramework" => (ArchitectureLayer.SharedWebFramework, null),
            _ => ClassifyModuleProject(repositoryRoot, projectPath, projectName),
        };
    }

    private static (ArchitectureLayer Layer, string? ModuleName) ClassifyModuleProject(
        string repositoryRoot,
        string projectPath,
        string projectName)
    {
        string relativePath = Path.GetRelativePath(repositoryRoot, projectPath);
        string[] segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        int modulesIndex = Array.IndexOf(segments, "Modules");

        if (modulesIndex < 0 || modulesIndex + 1 >= segments.Length)
        {
            return (ArchitectureLayer.Unknown, null);
        }

        string moduleName = segments[modulesIndex + 1];

        if (string.Equals(projectName, $"{moduleName}.Contracts", StringComparison.Ordinal))
        {
            return (ArchitectureLayer.Contracts, moduleName);
        }

        if (string.Equals(projectName, $"{moduleName}.Domain", StringComparison.Ordinal))
        {
            return (ArchitectureLayer.Domain, moduleName);
        }

        if (string.Equals(projectName, $"{moduleName}.Application", StringComparison.Ordinal))
        {
            return (ArchitectureLayer.Application, moduleName);
        }

        if (string.Equals(projectName, $"{moduleName}.Infrastructure", StringComparison.Ordinal))
        {
            return (ArchitectureLayer.Infrastructure, moduleName);
        }

        if (string.Equals(projectName, $"{moduleName}.Presentation", StringComparison.Ordinal))
        {
            return (ArchitectureLayer.Presentation, moduleName);
        }

        return (ArchitectureLayer.Unknown, moduleName);
    }

    private static IReadOnlyCollection<string> ReadProjectReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);

        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => NormalizePath(Path.GetFullPath(include!, Path.GetDirectoryName(projectPath)!)))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<ArchitectureModule> DiscoverModules(
        string repositoryRoot,
        IReadOnlyCollection<ArchitectureProject> projects)
    {
        string modulesRoot = Path.Combine(repositoryRoot, "src", "Product", "Modules");

        return Directory
            .EnumerateDirectories(modulesRoot)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .Select(moduleName => CreateModule(moduleName, projects))
            .ToArray();
    }

    private static ArchitectureModule CreateModule(
        string moduleName,
        IReadOnlyCollection<ArchitectureProject> projects)
    {
        ArchitectureProject? Find(ArchitectureLayer layer)
        {
            return projects.SingleOrDefault(project =>
                project.Layer == layer &&
                string.Equals(project.ModuleName, moduleName, StringComparison.Ordinal));
        }

        return new ArchitectureModule(
            moduleName,
            Find(ArchitectureLayer.Contracts),
            Find(ArchitectureLayer.Domain),
            Find(ArchitectureLayer.Application),
            Find(ArchitectureLayer.Infrastructure),
            Find(ArchitectureLayer.Presentation));
    }

    private static string FindRepositoryRoot(string startPath)
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);

        while (directory is not null)
        {
            string packagesProps = Path.Combine(directory.FullName, "Directory.Packages.props");
            string sourceDirectory = Path.Combine(directory.FullName, "src");

            if (File.Exists(packagesProps) && Directory.Exists(sourceDirectory))
            {
                return NormalizePath(directory.FullName);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private ArchitectureProject? FindProject(string referencePath)
    {
        return _projectsByPath.TryGetValue(referencePath, out var reference)
            ? reference
            : null;
    }
}

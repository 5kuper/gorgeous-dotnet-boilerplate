using ProjectName.ArchitectureTests.Support;

namespace ProjectName.ArchitectureTests.ProjectModel;

internal static class AllowedProjectReferences
{
    public static ArchitectureRuleResult Validate(ArchitectureProjectGraph graph)
    {
        var result = new ArchitectureRuleResult();

        foreach (var project in graph.Projects)
        {
            foreach (string unknownReferencePath in graph.GetUnknownReferencePaths(project))
            {
                result.Add(
                    "Project reference matrix",
                    project.Name,
                    ToRelativePath(graph.RepositoryRoot, unknownReferencePath),
                    "Source projects may reference only known source projects covered by the architecture model.");
            }

            foreach (var dependency in graph.GetReferences(project))
            {
                if (IsAllowed(project, dependency))
                {
                    continue;
                }

                result.Add(
                    "Project reference matrix",
                    project.Name,
                    dependency.Name,
                    DescribeAllowedReferences(project));
            }
        }

        return result;
    }

    private static bool IsAllowed(ArchitectureProject project, ArchitectureProject dependency)
    {
        return project.Layer switch
        {
            ArchitectureLayer.Contracts => dependency.Layer is ArchitectureLayer.SharedCore,
            ArchitectureLayer.Domain => dependency.Layer is ArchitectureLayer.SharedCore,
            ArchitectureLayer.Application => AllowsApplicationReference(project, dependency),
            ArchitectureLayer.Infrastructure => AllowsInfrastructureReference(project, dependency),
            ArchitectureLayer.Presentation => AllowsPresentationReference(project, dependency),
            ArchitectureLayer.SharedCore => false,
            ArchitectureLayer.SharedApplication => dependency.Layer is ArchitectureLayer.SharedCore,
            ArchitectureLayer.SharedWebFramework => dependency.Layer is
                ArchitectureLayer.SharedApplication or
                ArchitectureLayer.SharedCore,
            ArchitectureLayer.Host => AllowsHostReference(dependency),
            ArchitectureLayer.Persistence => AllowsPersistenceReference(dependency),
            _ => false,
        };
    }

    private static bool AllowsApplicationReference(ArchitectureProject project, ArchitectureProject dependency)
    {
        if (dependency.Layer is ArchitectureLayer.SharedApplication or ArchitectureLayer.SharedCore)
        {
            return true;
        }

        if (dependency.Layer is ArchitectureLayer.Contracts)
        {
            return true;
        }

        return project.IsOwnModuleProject(dependency) && dependency.Layer is ArchitectureLayer.Domain;
    }

    private static bool AllowsInfrastructureReference(ArchitectureProject project, ArchitectureProject dependency)
    {
        if (dependency.Layer is ArchitectureLayer.SharedApplication or ArchitectureLayer.SharedCore)
        {
            return true;
        }

        if (dependency.Layer is ArchitectureLayer.Contracts)
        {
            return true;
        }

        return project.IsOwnModuleProject(dependency) &&
               dependency.Layer is ArchitectureLayer.Domain or ArchitectureLayer.Application;
    }

    private static bool AllowsPresentationReference(ArchitectureProject project, ArchitectureProject dependency)
    {
        if (dependency.Layer is ArchitectureLayer.SharedWebFramework)
        {
            return true;
        }

        return project.IsOwnModuleProject(dependency) &&
               dependency.Layer is ArchitectureLayer.Application or ArchitectureLayer.Contracts;
    }

    private static bool AllowsHostReference(ArchitectureProject dependency)
    {
        if (dependency.Layer is ArchitectureLayer.Persistence or ArchitectureLayer.SharedWebFramework)
        {
            return true;
        }

        return dependency.BelongsToModule &&
               dependency.Layer is
                   ArchitectureLayer.Contracts or
                   ArchitectureLayer.Application or
                   ArchitectureLayer.Infrastructure or
                   ArchitectureLayer.Presentation;
    }

    private static bool AllowsPersistenceReference(ArchitectureProject dependency)
    {
        if (dependency.Layer is ArchitectureLayer.SharedCore)
        {
            return true;
        }

        return dependency.BelongsToModule &&
               dependency.Layer is ArchitectureLayer.Application or ArchitectureLayer.Infrastructure;
    }

    private static string DescribeAllowedReferences(ArchitectureProject project)
    {
        return project.Layer switch
        {
            ArchitectureLayer.Contracts => "Contracts may reference Shared.BuildingBlocks.Core only.",
            ArchitectureLayer.Domain => "Domain may reference Shared.BuildingBlocks.Core only.",
            ArchitectureLayer.Application =>
                "Application may reference own Domain, any module Contracts, Shared.BuildingBlocks.Application, and Shared.BuildingBlocks.Core.",
            ArchitectureLayer.Infrastructure =>
                "Infrastructure may reference own Domain/Application, any module Contracts, Shared.BuildingBlocks.Application, and Shared.BuildingBlocks.Core.",
            ArchitectureLayer.Presentation =>
                "Presentation may reference own Application/Contracts and Shared.WebFramework.",
            ArchitectureLayer.SharedCore => "Shared.BuildingBlocks.Core must not reference other source projects.",
            ArchitectureLayer.SharedApplication => "Shared.BuildingBlocks.Application may reference Shared.BuildingBlocks.Core only.",
            ArchitectureLayer.SharedWebFramework => "Shared.WebFramework may reference Shared.BuildingBlocks.Application and Shared.BuildingBlocks.Core only.",
            ArchitectureLayer.Host =>
                "Host may reference Persistence, Shared.WebFramework, and module Contracts/Application/Infrastructure/Presentation projects for composition.",
            ArchitectureLayer.Persistence =>
                "Persistence may reference Shared.BuildingBlocks.Core and module Application/Infrastructure projects for persistence composition.",
            _ => "Project must match a known architecture layer.",
        };
    }

    private static string ToRelativePath(string repositoryRoot, string path)
    {
        return Path.GetRelativePath(repositoryRoot, path);
    }
}

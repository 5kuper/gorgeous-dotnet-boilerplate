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
            ArchitectureLayer.Contracts => dependency.Layer is ArchitectureLayer.GorgeousAbstractions,
            ArchitectureLayer.Domain => dependency.Layer is ArchitectureLayer.GorgeousAbstractions or ArchitectureLayer.SharedKernel,
            ArchitectureLayer.Application => AllowsApplicationReference(project, dependency),
            ArchitectureLayer.Infrastructure => AllowsInfrastructureReference(project, dependency),
            ArchitectureLayer.Presentation => AllowsPresentationReference(project, dependency),
            ArchitectureLayer.GorgeousAbstractions => false,
            ArchitectureLayer.GorgeousWeb => dependency.Layer is ArchitectureLayer.GorgeousAbstractions,
            ArchitectureLayer.SharedKernel => false,
            ArchitectureLayer.SharedAppModel => dependency.Layer is ArchitectureLayer.GorgeousAbstractions,
            ArchitectureLayer.SharedConventions => false,
            ArchitectureLayer.Host => AllowsHostReference(dependency),
            ArchitectureLayer.Persistence => AllowsPersistenceReference(dependency),
            _ => false,
        };
    }

    private static bool AllowsApplicationReference(ArchitectureProject project, ArchitectureProject dependency)
    {
        if (dependency.Layer is
            ArchitectureLayer.GorgeousAbstractions or
            ArchitectureLayer.SharedAppModel or
            ArchitectureLayer.SharedKernel)
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
        if (dependency.Layer is
            ArchitectureLayer.GorgeousAbstractions or
            ArchitectureLayer.SharedAppModel or
            ArchitectureLayer.SharedKernel)
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
        if (dependency.Layer is ArchitectureLayer.GorgeousAbstractions or ArchitectureLayer.GorgeousWeb or ArchitectureLayer.SharedConventions)
        {
            return true;
        }

        return project.IsOwnModuleProject(dependency) &&
               dependency.Layer is ArchitectureLayer.Application or ArchitectureLayer.Contracts;
    }

    private static bool AllowsHostReference(ArchitectureProject dependency)
    {
        if (dependency.Layer is
            ArchitectureLayer.Persistence or
            ArchitectureLayer.GorgeousAbstractions or
            ArchitectureLayer.GorgeousWeb or
            ArchitectureLayer.SharedConventions)
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
        if (dependency.Layer is ArchitectureLayer.GorgeousAbstractions or ArchitectureLayer.SharedKernel)
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
            ArchitectureLayer.Contracts => "Contracts may reference Gorgeous.Abstractions only.",
            ArchitectureLayer.Domain => "Domain may reference Gorgeous.Abstractions and Shared.Kernel only.",
            ArchitectureLayer.Application =>
                "Application may reference own Domain, any module Contracts, Gorgeous.Abstractions, Shared.AppModel, and Shared.Kernel.",
            ArchitectureLayer.Infrastructure =>
                "Infrastructure may reference own Domain/Application, any module Contracts, Gorgeous.Abstractions, Shared.AppModel, and Shared.Kernel.",
            ArchitectureLayer.Presentation =>
                "Presentation may reference own Application/Contracts, Gorgeous.Abstractions, Gorgeous.Web, and shared conventions constants.",
            ArchitectureLayer.GorgeousAbstractions => "Gorgeous.Abstractions must not reference other source projects.",
            ArchitectureLayer.GorgeousWeb => "Gorgeous.Web may reference Gorgeous.Abstractions only.",
            ArchitectureLayer.SharedKernel => "Shared.Kernel must not reference other source projects.",
            ArchitectureLayer.SharedAppModel => "Shared.AppModel may reference Gorgeous.Abstractions only.",
            ArchitectureLayer.SharedConventions => "Shared.Conventions must not reference other source projects.",
            ArchitectureLayer.Host =>
                "Host may reference Persistence, Gorgeous.Abstractions, Gorgeous.Web, shared conventions constants, and module Contracts/Application/Infrastructure/Presentation projects for composition.",
            ArchitectureLayer.Persistence =>
                "Persistence may reference Gorgeous.Abstractions, Shared.Kernel, and module Application/Infrastructure projects for persistence composition.",
            _ => "Project must match a known architecture layer.",
        };
    }

    private static string ToRelativePath(string repositoryRoot, string path)
    {
        return Path.GetRelativePath(repositoryRoot, path);
    }
}

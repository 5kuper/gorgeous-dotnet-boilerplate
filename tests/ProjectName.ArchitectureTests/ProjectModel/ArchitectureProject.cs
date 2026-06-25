namespace ProjectName.ArchitectureTests.ProjectModel;

internal sealed record ArchitectureProject(
    string Name,
    string Path,
    ArchitectureLayer Layer,
    string? ModuleName,
    IReadOnlyCollection<string> ReferencePaths)
{
    public bool BelongsToModule => ModuleName is not null;

    public bool IsOwnModuleProject(ArchitectureProject dependency)
    {
        return BelongsToModule &&
               dependency.BelongsToModule &&
               string.Equals(ModuleName, dependency.ModuleName, StringComparison.Ordinal);
    }
}

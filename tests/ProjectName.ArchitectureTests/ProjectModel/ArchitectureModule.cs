namespace ProjectName.ArchitectureTests.ProjectModel;

internal sealed record ArchitectureModule(
    string Name,
    ArchitectureProject? Contracts,
    ArchitectureProject? Domain,
    ArchitectureProject? Application,
    ArchitectureProject? Infrastructure,
    ArchitectureProject? Presentation)
{
    public IReadOnlyList<string> MissingProjects
    {
        get
        {
            var missing = new List<string>();

            AddIfMissing(Contracts, $"{Name}.Contracts");
            AddIfMissing(Domain, $"{Name}.Domain");
            AddIfMissing(Application, $"{Name}.Application");
            AddIfMissing(Infrastructure, $"{Name}.Infrastructure");
            AddIfMissing(Presentation, $"{Name}.Presentation");

            return missing;

            void AddIfMissing(ArchitectureProject? project, string expectedProjectName)
            {
                if (project is null)
                {
                    missing.Add(expectedProjectName);
                }
            }
        }
    }
}

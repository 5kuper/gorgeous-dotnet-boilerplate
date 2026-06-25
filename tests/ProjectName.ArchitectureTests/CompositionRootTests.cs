using ProjectName.ArchitectureTests.Support;

namespace ProjectName.ArchitectureTests;

public sealed class CompositionRootTests
{
    private static readonly string[] BusinessImplementationSuffixes =
    [
        "CommandHandler",
        "QueryHandler",
        "Repository",
        "DbContext",
    ];

    [Fact]
    public void Host_should_not_define_module_implementation_types()
    {
        string[] offendingTypes = ArchitectureAssemblies.Host
            .GetTypes()
            .Where(type => BusinessImplementationSuffixes.Any(suffix => type.Name.EndsWith(suffix, StringComparison.Ordinal)))
            .Select(type => type.FullName ?? type.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            offendingTypes.Length == 0,
            "Host is the composition root. Move handlers, repositories, and DbContexts into modules or infrastructure." +
            Environment.NewLine +
            string.Join(Environment.NewLine, offendingTypes));
    }
}

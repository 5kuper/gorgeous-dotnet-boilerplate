namespace ProjectName.ArchitectureTests.Support;

internal static class ArchitectureAssert
{
    public static void ShouldBeValid(this ArchitectureRuleResult result)
    {
        Assert.True(result.IsValid, result.ToString());
    }
}

using Xunit;

namespace Shared.IntegrationTesting.WebApi;

public static class ProblemDetailsAssertions
{
    public static void ShouldBeProblem(
        this ProblemDetailsSnapshot problem,
        string expectedCode,
        string expectedDetail,
        int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, problem.StatusCode);
        Assert.Equal(expectedDetail, problem.Detail);
        Assert.Equal(expectedCode, problem.Code);
    }
}

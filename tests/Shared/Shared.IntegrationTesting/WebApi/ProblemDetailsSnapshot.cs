namespace Shared.IntegrationTesting.WebApi;

public sealed record ProblemDetailsSnapshot(
    int StatusCode,
    string? Detail,
    string? Code);

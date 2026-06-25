using ProjectName.ArchitectureTests.Support;

namespace ProjectName.ArchitectureTests;

public sealed class LayerPurityTests
{
    private static readonly string[] DependenciesForbiddenInDomainCode =
    [
        "MediatR",
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Extensions",
        "Microsoft.AspNetCore.Identity",
    ];

    private static readonly string[] DependenciesForbiddenInApplicationCode =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore.Identity",
        "Microsoft.IdentityModel",
        "System.IdentityModel",
        "Stripe",
    ];

    [Fact]
    public void Domain_should_stay_framework_free()
    {
        var domainAssemblies = ArchitectureAssemblies.Modules.Select(module => module.Domain);

        var result = NetArchTestRule.AssembliesShouldNotDependOn(
            domainAssemblies,
            "Domain purity",
            DependenciesForbiddenInDomainCode,
            "Domain may depend on domain code and Shared.BuildingBlocks.Core, not frameworks or mediator abstractions.");

        result.ShouldBeValid();
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_transport_frameworks()
    {
        var applicationAssemblies = ArchitectureAssemblies.Modules.Select(module => module.Application);

        var result = NetArchTestRule.AssembliesShouldNotDependOn(
            applicationAssemblies,
            "Application purity",
            DependenciesForbiddenInApplicationCode,
            "Application may use DI registration, MediatR contracts, own domain, and module contracts; EF Core, ASP.NET Core, Identity, and provider SDKs stay outside.");

        result.ShouldBeValid();
    }

    [Fact]
    public void Shared_core_should_stay_framework_free()
    {
        var result = NetArchTestRule.AssemblyShouldNotDependOn(
            ArchitectureAssemblies.SharedCore,
            "Shared core purity",
            DependenciesForbiddenInDomainCode,
            "Shared.BuildingBlocks.Core must stay framework-free.");

        result.ShouldBeValid();
    }

    [Fact]
    public void Shared_application_should_not_depend_on_infrastructure_or_transport_frameworks()
    {
        var result = NetArchTestRule.AssemblyShouldNotDependOn(
            ArchitectureAssemblies.SharedApplication,
            "Shared application purity",
            DependenciesForbiddenInApplicationCode,
            "Shared.BuildingBlocks.Application may use MediatR, but not EF Core, ASP.NET Core, Identity, or provider SDKs.");

        result.ShouldBeValid();
    }
}

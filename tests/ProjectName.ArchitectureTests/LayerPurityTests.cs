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
            "Domain may depend on domain code and Shared.Kernel, not frameworks or mediator abstractions.");

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
    public void Shared_kernel_should_stay_framework_free()
    {
        var result = NetArchTestRule.AssemblyShouldNotDependOn(
            ArchitectureAssemblies.SharedKernel,
            "Shared kernel purity",
            DependenciesForbiddenInDomainCode,
            "Shared.Kernel must stay framework-free.");

        result.ShouldBeValid();
    }

    [Fact]
    public void Gorgeous_abstractions_should_stay_aspnetcore_free()
    {
        var result = NetArchTestRule.AssemblyShouldNotDependOn(
            ArchitectureAssemblies.GorgeousAbstractions,
            "Gorgeous abstractions purity",
            ["Microsoft.AspNetCore"],
            "Gorgeous.Abstractions must stay portable and must not depend on ASP.NET Core.");

        result.ShouldBeValid();
    }

    [Fact]
    public void Gorgeous_libraries_should_not_depend_on_product_code()
    {
        string[] forbiddenProductNamespaces =
        [
            .. ArchitectureAssemblies.Modules.SelectMany(module => module.Assemblies.Select(assembly => assembly.GetName().Name!)),
            "Shared.Kernel",
            "Shared.AppModel",
            "Shared.Conventions",
            "ProjectName.Host",
            "ProjectName.Persistence",
        ];

        var result = NetArchTestRule.AssembliesShouldNotDependOn(
            [ArchitectureAssemblies.GorgeousAbstractions, ArchitectureAssemblies.GorgeousWeb],
            "Gorgeous library portability",
            forbiddenProductNamespaces,
            "Gorgeous.Abstractions and Gorgeous.Web must not depend on Product, Host, Persistence, or product-owned Shared projects.");

        result.ShouldBeValid();
    }

    [Fact]
    public void Shared_application_model_should_not_depend_on_infrastructure_or_transport_frameworks()
    {
        var result = NetArchTestRule.AssemblyShouldNotDependOn(
            ArchitectureAssemblies.SharedAppModel,
            "Shared application model purity",
            DependenciesForbiddenInApplicationCode,
            "Shared.AppModel may use MediatR, but not EF Core, ASP.NET Core, Identity, or provider SDKs.");

        result.ShouldBeValid();
    }
}

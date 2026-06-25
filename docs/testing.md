# Testing

## Test Layout

```text
tests/
|-- Modules/
|   |-- Auth/
|   |   |-- Auth.CoreTests/
|   |   `-- Auth.IntegrationTests/
|   `-- Users/
|       |-- Users.CoreTests/
|       `-- Users.IntegrationTests/
|-- ProjectName.ArchitectureTests/
`-- Shared/
    |-- Shared.IntegrationTesting/
    `-- Shared.TestKit/
```

## Test Project Roles

`*.CoreTests` cover domain and application behavior without web or database wiring unless the test explicitly needs it.

`*.IntegrationTests` cover module behavior through infrastructure, persistence, or HTTP-level boundaries.

`ProjectName.ArchitectureTests` covers solution-level rules such as module references, module boundaries, and forbidden dependencies.

`Shared.TestKit` contains reusable test helpers.

`Shared.IntegrationTesting` contains reusable integration test infrastructure.

## When To Add Tests

Add tests when a change affects:

- domain invariants;
- authentication or authorization behavior;
- refresh-token rotation;
- module contracts;
- persistence mappings or migrations;
- error mapping;
- cross-module workflows;
- architecture boundaries.

Small mechanical changes can stay lighter, but behavior changes should have focused coverage.

## Commands

Run all tests:

```powershell
dotnet test .\ProjectName.slnx
```

Run one test project:

```powershell
dotnet test .\tests\Modules\Auth\Auth.CoreTests\Auth.CoreTests.csproj
```

## Test Code Rules

- Keep module test projects under `tests/Modules/{ModuleName}`.
- Keep shared helpers under `tests/Shared`.
- Do not put production-only dependencies into `Shared.TestKit`.
- Keep architecture tests separate from module tests.
- Prefer testing public behavior over internal implementation details.

## Architecture Tests

Architecture tests are executable documentation for the modular monolith boundaries. They focus on dependency rules, not style preferences.

They currently cover:

- the expected module project layout under `src/Product/Modules/{ModuleName}`;
- allowed direct `ProjectReference` directions between layers;
- cross-module access only through `*.Contracts`;
- framework leakage rules for Domain, Application, Contracts, and shared building blocks;
- minimal Host guardrails so business handlers, repositories, and DbContexts stay out of the composition root.

When adding a new module, keep the standard project shape:

```text
{ModuleName}.Contracts/
Core/
|-- {ModuleName}.Domain/
`-- {ModuleName}.Application/
Adapters/
|-- {ModuleName}.Infrastructure/
`-- {ModuleName}.Presentation/
```

Architecture tests reference module projects with a glob in `tests/ProjectName.ArchitectureTests/ProjectName.ArchitectureTests.csproj`, and assembly-level checks discover module assemblies from the project graph. If a module assembly cannot be loaded, make sure the module project follows the standard project naming and layout.

If a new dependency direction is needed, update the allowed reference matrix in `tests/ProjectName.ArchitectureTests/ProjectModel/AllowedProjectReferences.cs` and make the reason clear in the rule text. Prefer adding a narrow positive rule over weakening a broad forbidden-dependency check.

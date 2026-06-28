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

## Test Project Structure

Module test projects separate behavior specifications from local utilities:

```text
{Module}.CoreTests/
|-- Tests/
|   |-- Domain/
|   `-- Application/
`-- Support/
    |-- Assertions/
    |-- Builders/
    |-- Fixtures/
    `-- TestDoubles/

{Module}.IntegrationTests/
|-- Persistence/
|   |-- Support/
|   `-- *Tests.cs
`-- WebApi/
    |-- Support/
    `-- *Tests.cs
```

Core test projects use top-level `Tests/` for test classes and `Support/` for local utilities. Integration test projects group by tested boundary first: keep persistence tests and their utilities under `Persistence/`, and web API tests and their utilities under `WebApi/`.

Support folders should contain builders, fake services, fixtures, seed data, custom clients, and local assertion helpers. Test classes should stay next to their tested boundary, outside that boundary's `Support/` folder.

Keep utilities local to a test project while they are module-specific. Move helpers into `Shared.TestKit` or `Shared.IntegrationTesting` only when they are used by multiple test projects.

`Shared.TestKit` groups reusable helpers by test role:

```text
Shared.TestKit/
|-- Assertions/
|-- TestData/
`-- TestDoubles/
```

`Shared.IntegrationTesting` groups reusable integration infrastructure by framework concern, such as database, messaging, authentication, and web API metadata helpers.

Prefer domain tests and integration tests as the main coverage. Add application tests selectively for use cases with meaningful orchestration, transaction boundaries, port coordination, or important failure-path mapping. Do not use `InternalsVisibleTo` by default; application tests should go through public commands, queries, contracts, MediatR, or HTTP-level boundaries.

## Testing A New Module

For each product module, add two module-level test projects:

```text
tests/Modules/{Module}/{Module}.CoreTests/
|-- Tests/
|   |-- Domain/
|   `-- Application/
`-- Support/

tests/Modules/{Module}/{Module}.IntegrationTests/
|-- Persistence/
|   |-- Support/
|   `-- *Tests.cs
`-- WebApi/
    |-- Support/
    `-- *Tests.cs
```

Start with domain and integration coverage. Add application tests only when the use case has meaningful orchestration, port coordination, transaction behavior, or failure-path mapping.

Minimum coverage for a new module:

- domain invariants, aggregate lifecycle, domain errors, and domain events;
- use cases only where orchestration is non-trivial;
- EF mappings, constraints, repository behavior, read models, and migrations;
- HTTP route mapping, authorization metadata, rate limits, validation, and error mapping;
- module contracts used by other modules;
- architecture boundaries through `ProjectName.ArchitectureTests`.

Keep new module helpers local first. Promote helpers to `Shared.TestKit` or `Shared.IntegrationTesting` only after at least two test projects need the same helper and the helper does not encode module-specific knowledge.

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

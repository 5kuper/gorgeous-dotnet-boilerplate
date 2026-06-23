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

`ProjectName.ArchitectureTests` covers solution-level rules such as module references, forbidden dependencies, and naming conventions.

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

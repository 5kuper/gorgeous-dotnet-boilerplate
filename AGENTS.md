# Agent Guidelines

## Project Shape

This repository is an ASP.NET Core modular monolith template.

- `src/ProjectName.Host` is the composition root. Keep it focused on middleware, service registration, endpoint mapping, and host configuration.
- `src/Product/Modules/{ModuleName}` contains product modules.
- `src/Product/Shared` contains stable cross-module building blocks and web framework helpers.
- `src/Infrastructure/ProjectName.Persistence` contains application-level persistence composition, database initialization, and cross-DbContext transactions.
- `tests` mirrors the product/module layout.
- Project documentation starts at `docs/README.md`; update it when architecture, module boundaries, persistence ownership, authentication behavior, or public API conventions change.

## Module Boundaries

Each module follows this layout:

```text
{ModuleName}.Contracts/
Core/
|-- {ModuleName}.Domain/
`-- {ModuleName}.Application/
Adapters/
|-- {ModuleName}.Infrastructure/
`-- {ModuleName}.Presentation/
```

- `Domain` contains entities, aggregates, domain errors/events, domain services, and repository contracts.
- `Application` contains use cases, MediatR handlers, validators, and module-specific ports.
- `Contracts` contains stable module APIs for other modules. Do not put HTTP models, EF Core entities, Identity types, or provider-specific errors here.
- `Infrastructure` contains EF Core, repository implementations, external adapters, Identity integration, and read projections.
- `Presentation` contains Minimal API endpoint mapping and HTTP request/response models.
- Modules may talk to other modules through `Contracts` projects only.
- Do not reference another module's `Domain`, `Application`, `Infrastructure`, or `Presentation` project from a module.

## Implementation Rules

- Keep business logic out of `ProjectName.Host` and endpoint handlers.
- Keep EF Core, ASP.NET Core Identity, provider SDKs, and transport concerns out of domain/application code.
- Return `Result` or `Result<T>` for business failures; reserve exceptions for technical failures and programming errors.
- Use shared building blocks only for generic, stable concepts that are genuinely shared across modules.
- Keep `CancellationToken` parameter names as `ct`.
- Prefer existing folder conventions and registration patterns over new abstractions.

## Packages And Build Policy

- Keep NuGet package versions in `Directory.Packages.props`; project files should use `PackageReference` without `Version`.
- Keep shared MSBuild policy in `Directory.Build.props`; do not duplicate common properties in individual project files.
- Do not disable nullable, analyzers, or code-style enforcement in a project file without documenting why.
- When changing package references or central package versions, run a vulnerability audit.
- Do not add build outputs, IDE state, user-local files, logs, coverage output, or temporary files to the repository.

## Testing And Verification

- `*.CoreTests` cover domain and application behavior.
- `*.IntegrationTests` cover infrastructure, persistence, or HTTP-level behavior.
- `ProjectName.ArchitectureTests` covers solution-level dependency and naming rules.
- Add focused tests for changes to domain invariants, auth/authorization, refresh-token rotation, module contracts, persistence mappings, migrations, error mapping, cross-module workflows, or architecture boundaries.

Run the relevant checks before handing off:

```powershell
dotnet restore .\ProjectName.slnx
dotnet build .\ProjectName.slnx --no-restore
dotnet test .\ProjectName.slnx --no-build
dotnet list .\ProjectName.slnx package --vulnerable --include-transitive
```

If a command cannot be run, state that explicitly with the reason.

# Module Development

## Add A Module

Create the module under:

```text
src/Product/Modules/{ModuleName}/
```

Use this project layout:

```text
{ModuleName}.Contracts/
Core/
|-- {ModuleName}.Domain/
`-- {ModuleName}.Application/
Adapters/
|-- {ModuleName}.Infrastructure/
`-- {ModuleName}.Presentation/
```

Add the projects to `ProjectName.slnx`, then compose the module from the host:

- register application and infrastructure services from `ProjectName.Host/Composition/ModulesComposition.cs`;
- map presentation endpoints from `ProjectName.Host/Composition/EndpointsComposition.cs`;
- keep module-specific registration extension methods inside the module projects.

Keep the host responsible for composition only. Do not move module business logic into `ProjectName.Host`.

When a new NuGet package is needed, add its version to `Directory.Packages.props` and keep the project `PackageReference` without a `Version` attribute.

## Add A Use Case

Application use cases live under:

```text
src/Product/Modules/{ModuleName}/Core/{ModuleName}.Application/Features/
```

Group use cases by feature area. A feature area folder can contain several related commands, queries, handlers, and small support types.

The current modules usually keep the command or query record and its handler in the same `.cs` file.

Example:

```text
Features/
|-- Sessions/
|   |-- LoginWithPasswordCommand.cs
|   |-- RefreshTokenCommand.cs
|   |-- LogoutCommand.cs
|   `-- Support/
|       |-- AuthRules.cs
|       |-- AuthSession.cs
|       `-- TokenHashing.cs
`-- Roles/
    |-- AssignRoleCommand.cs
    |-- CreateRoleCommand.cs
    `-- RevokeRoleCommand.cs
```

Create a dedicated use-case folder only when a use case grows enough supporting code to justify it.

Use cases:

- orchestrate domain objects and repositories;
- call module-specific ports;
- return `Result` or `Result<T>` for business failures;
- keep HTTP and EF Core details out of application code.

Simple application input checks can live in handlers. If a feature grows a dedicated validator, keep it in the same feature-area folder. Domain entities still enforce invariants that must always hold.

## Add Domain Behavior

Domain code lives under:

```text
src/Product/Modules/{ModuleName}/Core/{ModuleName}.Domain/
```

Recommended structure:

```text
Entities/
Foundation/
|-- Errors/
|-- Events/
`-- Primitives/
Repositories/
Services/
```

Repository interfaces belong in the domain layer. Repository implementations belong in infrastructure.

Introduce value objects, domain services, and extra subfolders only when they remove real complexity or protect important invariants.

## Add A Module Contract

Add contracts under:

```text
src/Product/Modules/{ModuleName}/{ModuleName}.Contracts/
```

Use command-style contracts when another module asks the owning module to perform a business operation.

Use read projection contracts when another module needs read-only data owned by this module.

Contracts should expose stable module language. They must not expose:

- EF Core entities;
- ASP.NET Core Identity types;
- HTTP request or response models;
- database table models;
- provider-specific errors.

## Add An Endpoint

Endpoints live in the module presentation project:

```text
src/Product/Modules/{ModuleName}/Adapters/{ModuleName}.Presentation/
```

The current modules place HTTP endpoint code under `WebApi/`, with request and response models under `WebApi/Requests` and `WebApi/Responses` when separate models are needed.

Endpoint handlers should:

- map HTTP requests to application commands or queries;
- pass `CancellationToken ct`;
- use shared result-to-HTTP helpers;
- attach authorization and rate limiting at group or endpoint level;
- keep request and response models inside presentation.

Do not reuse HTTP models as module contracts.

## Add Persistence

Module-owned infrastructure code lives under:

```text
src/Product/Modules/{ModuleName}/Adapters/{ModuleName}.Infrastructure/
```

Use infrastructure for:

- DbContexts;
- EF Core configurations;
- migrations;
- repository implementations;
- external provider implementations;
- read projection implementations;
- module infrastructure registration.

Application code should depend on repository contracts and ports, not on EF Core.

Cross-module persistence composition lives in:

```text
src/Infrastructure/ProjectName.Persistence/
```

Use this application-level infrastructure area for shared database connection setup, DbContext registration, database initialization, and transactions that coordinate multiple module DbContexts.

When a workflow needs to update multiple DbContexts, expose a port in the application layer and implement the transaction in application-level infrastructure.

## Add A Migration

Place migrations with the owning infrastructure project.

The existing modules keep migrations under:

```text
Adapters/{ModuleName}.Infrastructure/Persistence/Migrations/
```

Keep ownership visible. Do not place all module migrations into one unrelated infrastructure project unless that project clearly owns the DbContext.

## Add Shared Code

Use shared projects only for stable cross-module concepts.

Use `Shared.BuildingBlocks.Core` for domain-neutral primitives.
Use `Shared.BuildingBlocks.Application` for application-neutral abstractions.
Use `Shared.WebFramework` for web-specific conventions and helpers.

If a type is shared only because two modules currently look similar, keep it local until a stable abstraction is clear.

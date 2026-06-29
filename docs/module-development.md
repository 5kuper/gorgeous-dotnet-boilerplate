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

Use `Gorgeous.Abstractions.Results` for `Result`, `Result<T>`, `Error`, and `ErrorType`.

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
- use `Gorgeous.Web` result-to-HTTP helpers;
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
src/RootInfrastructure/ProjectName.Persistence/
```

Use this application-level infrastructure area for shared database connection setup, DbContext registration, database initialization, and transactions that coordinate multiple module DbContexts.

When a workflow needs to update multiple DbContexts, expose a port in the application layer and implement the transaction in application-level infrastructure.

Non-persistence application-level infrastructure lives outside the persistence project. For example, product AI scenario routing and AI infrastructure registration live under:

```text
src/RootInfrastructure/ProjectName.Ai/
```

## Add A Migration

Place migrations with the owning infrastructure project.

The existing modules keep migrations under:

```text
Adapters/{ModuleName}.Infrastructure/Persistence/Migrations/
```

Keep ownership visible. Do not place all module migrations into one unrelated infrastructure project unless that project clearly owns the DbContext.

## Add Shared Code

Use shared projects only for stable cross-module concepts.

Use `src/Libraries` for portable libraries that can move between projects:

- `Gorgeous.Abstractions` for portable result/current-user/clock abstractions.
- `Gorgeous.Abstractions/Ai` for provider-neutral AI request/response contracts.
- `Gorgeous.Ai` for reusable AI integration infrastructure. It currently contains provider-neutral client resolution and DI registration boundaries; future provider adapters may live behind the same abstractions.
- `Gorgeous.Web` for ASP.NET Core helpers and adapters over those abstractions.

Use `src/Product/Shared` for product-owned building blocks:

- `Shared.Kernel` for the product-owned shared domain kernel.
- `Shared.Kernel/Ai` for product AI scenario vocabulary.
- `Shared.AppModel` for the shared application-layer programming model, such as commands, queries, handlers, and `IUnitOfWork`.
- `Shared.AppModel/Ai` for product-facing AI scenario ports.

Keep shared domain primitives such as `Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`, and `IRepository` under `src/Product/Shared/Kernel/BuildingBlocks`.

Use `src/Product/Shared/Conventions` for product-owned API and cross-cutting naming conventions such as authorization policy names, rate-limit policy names, claim names, headers, and route identifiers.

If a type is shared only because two modules currently look similar, keep it local until a stable abstraction is clear.

## Add AI Usage

Use `IAiScenarioChatCompletionClient` from `Shared.AppModel/Ai` when product code needs chat completion. Pass an `AiScenario` from `Shared.Kernel/Ai`; do not choose concrete provider names inside modules.

Map scenarios to provider/model selections under `Ai:Scenarios` in host configuration. Future provider packages can register `IAiChatCompletionClient` implementations through `Gorgeous.Ai`, but modules must not reference `Gorgeous.Ai`, OpenAI, GigaChat, or provider SDKs directly.

Keep scenario routing in `ProjectName.Ai`, not in module code or `ProjectName.Persistence`.

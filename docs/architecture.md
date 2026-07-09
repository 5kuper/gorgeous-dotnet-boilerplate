# Architecture

## Overview

The project is a modular monolith.

Business capabilities live in modules. The host composes modules, shared framework services, authentication, authorization, rate limiting, and persistence. The host should not contain module business logic.

```text
ProjectName.Host
|-- composes modules
|-- configures ASP.NET Core middleware
`-- maps module endpoints

Product/Modules
|-- Users
`-- Auth

Libraries
|-- Gorgeous.Abstractions
|-- Gorgeous.Ai
`-- Gorgeous.Web

Product/Shared
|-- Kernel
|   `-- Shared.Kernel
|-- AppModel
|   `-- Shared.AppModel
`-- Conventions
    `-- Shared.Conventions

RootInfrastructure
|-- ProjectName.Ai
`-- ProjectName.Persistence
```

## Module Layout

Each product module follows the same shape:

```text
src/Product/Modules/{ModuleName}/
|-- {ModuleName}.Contracts/
|-- Core/
|   |-- {ModuleName}.Domain/
|   `-- {ModuleName}.Application/
`-- Adapters/
    |-- {ModuleName}.Infrastructure/
    `-- {ModuleName}.Presentation/
```

`Domain` contains entities, aggregates, domain errors, domain events, domain services, and repository contracts.

`Application` contains use cases, MediatR handlers, validators, and module-specific ports.

`Contracts` contains the stable module API used by other modules. It is not an HTTP layer.

`Infrastructure` contains EF Core, repository implementations, external service adapters, Identity integration, and read projection implementations.

`Presentation` contains Minimal API endpoint mapping and HTTP request/response models.

## Dependency Rules

- `Domain` depends only on shared kernel building blocks and portable result abstractions.
- `Application` depends on its domain, module contracts when needed, shared application model building blocks, and portable application/result abstractions.
- `Infrastructure` depends on the module domain/application contracts it implements.
- `Presentation` depends on the module application layer, module contracts, `Gorgeous.Web`, and product-owned web policy names when needed.
- Modules communicate with other modules through `Contracts` projects only.
- A module must not reference another module's `Application`, `Domain`, `Infrastructure`, or `Presentation` project.
- HTTP request and response models are not module contracts.

## Portable Libraries

Reusable libraries that are intended to move between projects live under:

```text
src/Libraries/
```

`Gorgeous.Abstractions` contains portable abstractions:

- generic AI chat completion contracts and model/provider keys;
- `Result`
- `Result<T>`
- `Error`
- `ErrorType`
- `IClock`
- `ICurrentUser`

It must stay framework-free and must not depend on ASP.NET Core or product code.

`Gorgeous.Ai` is the reusable AI integration library. At this stage it contains provider-neutral infrastructure:

- a provider-keyed chat completion client resolver;
- DI registration helpers for future provider packages.

It references `Gorgeous.Abstractions` only today. Future provider adapters may add provider SDK, HTTP payload, authentication, and provider API implementation details behind `IAiChatCompletionClient`. Those adapters must not know about product scenarios or depend on product-owned projects.

`Gorgeous.Web` contains ASP.NET Core helpers and adapters for those abstractions:

- result-to-HTTP mapping;
- `ProblemDetails` helpers;
- `HttpCurrentUser`;
- `SystemClock`;
- generic validated options registration.

`Gorgeous.Web` references `Gorgeous.Abstractions` and ASP.NET Core, but it must not depend on product modules or product-owned shared projects.

## Shared Building Blocks

Product-owned shared building blocks live under:

```text
src/Product/Shared/
```

They are part of this product's modular monolith and are not reusable library packages.

`Shared.Kernel` is the product-owned shared domain kernel. It contains domain primitives and explicitly shared domain concepts used by multiple modules:

- `Ai/AiScenario`
- `BuildingBlocks/Entity`
- `BuildingBlocks/AggregateRoot`
- `BuildingBlocks/ValueObject`
- `BuildingBlocks/IDomainEvent`
- `BuildingBlocks/IRepository`

`Shared.AppModel` contains the shared application-layer programming model:

- AI scenario ports such as `IAiScenarioModelResolver` and `IAiScenarioChatCompletionClient`
- feature flag access through `IFeatureGate`
- `ICommand`
- `ICommandHandler`
- `IQuery`
- `IQueryHandler`
- `IUnitOfWork`

Add a type to shared building blocks only when it is generic, stable, and genuinely shared across modules. Module-specific business language belongs in the module.

## AI Boundary

AI calls use a two-level boundary:

```text
Product code
  -> AiScenario
  -> IAiScenarioModelResolver
  -> AiModelSelection
  -> IAiChatCompletionClient
```

Product modules depend on `Shared.AppModel` ports and `Gorgeous.Abstractions` contracts. They must not reference `Gorgeous.Ai` or concrete provider infrastructure.

Scenario vocabulary lives in `Shared.Kernel/Ai`. Product-facing ports live in `Shared.AppModel/Ai`. Scenario routing is implemented in root infrastructure (`ProjectName.Ai`) because it composes product scenario configuration with portable AI provider clients.

Configuration is split by responsibility:

```json
{
  "Ai": {
    "Providers": {},
    "Scenarios": {
      "Example": {
        "Provider": "",
        "Model": ""
      }
    }
  }
}
```

`Ai:Providers` is reserved for future provider packages. `Ai:Scenarios` maps product scenarios to provider/model selections. Missing or unknown AI scenario configuration returns `Result` errors; it does not throw business-flow exceptions.

## Configuration Boundary

The host composes configuration from base app settings, root infrastructure files, module files, feature files, environment-specific files, development user secrets, optional Azure App Configuration, environment variables, and command line arguments.

Modules own their configuration sections and bind them to typed options in infrastructure. The host owns provider order and cloud configuration sources. Application and domain code should not read `IConfiguration` directly.

See [Configuration](configuration.md) for source order and file ownership.

## Feature Flags

Feature flag names live in `Shared.Conventions.AppFeatures`.

Application code depends on `Shared.AppModel.Abstractions.IFeatureGate`. The host implements that abstraction over Microsoft Feature Management.

Modules and application projects must not reference `Microsoft.FeatureManagement` or Azure packages. This keeps feature-flag provider choices in the composition root.

## Error Flow

Business failures flow as `Result` or `Result<T>`.

```text
Domain/Application
  -> Result or Result<T>
Presentation
  -> ProblemDetails HTTP response
```

Exceptions are reserved for technical failures and programming errors.

`Error` values include:

- stable module-scoped code;
- `ErrorType` for response mapping.
- `ErrorVisibility` for public disclosure policy.

Errors are sensitive by default. Mark an error as `Public` only when the code and message are safe to expose directly to API clients, such as simple validation failures.

HTTP error mapping belongs in `Gorgeous.Web`, not in domain or application code. The web layer has two result-to-HTTP paths:

- `ToHttpResult` is trusted and transparent. It maps the original `Error` into `ProblemDetails`.
- `ToPublicHttpResult` is public-safe. It first applies endpoint-specific disclosure masks, then exposes `Public` errors as-is, and otherwise returns a generic `Common.RequestFailed` response with the original error status category.

Module-specific disclosure masks live in module presentation projects. Auth masks are defined in `Auth.Presentation` and match by stable `Error.Code` strings so presentation does not reference domain error constants.

## Web Layer

The project uses Minimal APIs.

Module presentation projects map endpoint groups and translate HTTP models to application commands or queries. Endpoint handlers should stay thin and should not contain business rules.

Reusable ASP.NET Core helpers live in:

```text
src/Libraries/Gorgeous.Web/
```

This includes:

- current-user access;
- result-to-HTTP mapping;
- `ProblemDetails` helpers;
- clock implementation.

Product-owned API and cross-cutting naming conventions live in:

```text
src/Product/Shared/Conventions/
```

This keeps authorization and rate-limit policy names, claim names, header names, and similar product identifiers close to the product while leaving `Gorgeous.Web` reusable.

## Persistence

The project uses one database with multiple DbContexts.

`UsersDbContext` owns business user data.
`AuthDbContext` owns Identity data and refresh sessions.

Persistence ownership rules:

- each module writes its own tables;
- cross-module writes go through command-style module contracts;
- cross-module reads go through module contracts or approved read projections;
- application code depends on ports and repositories, not EF Core types;
- migrations stay with the module infrastructure project that owns the DbContext.

When one use case must update multiple DbContexts in the same database, application code uses a port and infrastructure coordinates the EF transaction.

Registration is the reference case:

```text
Auth.Application
  -> IRegistrationTransaction
      -> Users creates the business user
      -> Auth creates the Identity user
  -> commit
  -> send verification message after commit
```

## Security Model

ASP.NET Core Identity owns credentials, password validation, lockout, confirmation tokens, password reset tokens, and external-login extension points.

The project owns:

- business user lifecycle;
- business roles;
- refresh sessions;
- JWT claim selection;
- module contracts;
- public error behavior.

JWT signing keys, production connection strings, and email provider secrets must not be stored in source control.

## Code Style

`CancellationToken` parameters and local delegate parameters are named `ct`.

```csharp
Task SaveChangesAsync(CancellationToken ct = default);
```

Shared build and analyzer policy belongs in `Directory.Build.props`.
NuGet package versions belong in `Directory.Packages.props`.

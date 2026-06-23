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

Product/Shared
|-- BuildingBlocks
`-- Shared.WebFramework

Infrastructure
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

- `Domain` depends only on shared core building blocks.
- `Application` depends on its domain, module contracts when needed, and shared application building blocks.
- `Infrastructure` depends on the module domain/application contracts it implements.
- `Presentation` depends on the module application layer, module contracts, and shared web framework.
- Modules communicate with other modules through `Contracts` projects only.
- A module must not reference another module's `Application`, `Domain`, `Infrastructure`, or `Presentation` project.
- HTTP request and response models are not module contracts.

## Shared Building Blocks

Shared building blocks live under:

```text
src/Product/Shared/BuildingBlocks/
```

`Shared.BuildingBlocks.Core` contains domain-oriented primitives:

- `Entity`
- `AggregateRoot`
- `ValueObject`
- `IDomainEvent`
- `IRepository`
- `Error`
- `Result`
- `Result<T>`

`Shared.BuildingBlocks.Application` contains application-oriented abstractions:

- `ICommand`
- `ICommandHandler`
- `IQuery`
- `IQueryHandler`
- `IClock`
- `ICurrentUser`
- `IUnitOfWork`

Add a type to shared building blocks only when it is generic, stable, and genuinely shared across modules. Module-specific business language belongs in the module.

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
- safe public message;
- `ErrorType` for response mapping.

HTTP error mapping belongs in `Shared.WebFramework`, not in domain or application code.

## Web Layer

The project uses Minimal APIs.

Module presentation projects map endpoint groups and translate HTTP models to application commands or queries. Endpoint handlers should stay thin and should not contain business rules.

Reusable web concerns live in:

```text
src/Product/Shared/Shared.WebFramework/
```

This includes:

- current-user access;
- result-to-HTTP mapping;
- `ProblemDetails` helpers;
- rate limit policy names;
- authorization policy names;
- clock implementation.

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

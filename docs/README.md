# Project Documentation

This directory describes the project produced from the template.

It is intentionally written as project documentation, not template documentation. After the repository is copied and `ProjectName` is renamed, these documents should still describe the application architecture, module boundaries, development workflow, and test layout.

## Reading Order

1. [Getting Started](getting-started.md) explains how to configure, run, and smoke-check the application.
2. [Configuration](configuration.md) explains source order, module-owned config files, validated options, feature flags, and secrets.
3. [Architecture](architecture.md) explains the module model, portable libraries, dependency rules, persistence ownership, HTTP conventions, and shared building blocks.
4. [Module Development](module-development.md) explains how to add modules, use cases, contracts, endpoints, persistence code, and migrations.
5. [Users And Auth](users-auth.md) documents the reference Users/Auth implementation that ships with the project.
6. [Azure Deployment](azure-deployment.md) explains Azure App Configuration, Key Vault readiness, and managed identity assumptions.
7. [Testing](testing.md) explains the test project layout and when each test type belongs.

## Documentation Rules

- Keep these documents product-facing.
- Prefer explaining project behavior and decisions over describing the template.
- Update documentation when module boundaries, configuration ownership, persistence ownership, authentication behavior, Azure behavior, feature flags, or public API conventions change.

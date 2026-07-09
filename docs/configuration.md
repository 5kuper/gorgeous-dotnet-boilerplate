# Configuration

## Source Order

The host owns configuration composition in `ProjectName.Host`.

Configuration providers are added in this order. Later sources override earlier sources:

1. `appsettings.json`
2. `src/ProjectName.Host/config/root/*.config.json`
3. `src/ProjectName.Host/config/modules/*.config.json`
4. `src/ProjectName.Host/config/features/*.config.json`
5. `appsettings.{Environment}.json`
6. `src/ProjectName.Host/config/root/*.{Environment}.config.json`
7. `src/ProjectName.Host/config/modules/*.{Environment}.config.json`
8. `src/ProjectName.Host/config/features/*.{Environment}.config.json`
9. user secrets in Development
10. Azure App Configuration, when enabled
11. environment variables
12. command line arguments

This keeps normal ASP.NET Core override behavior while letting modules own their configuration files.

## File Ownership

Root infrastructure configuration lives under:

```text
src/ProjectName.Host/config/root/
```

Current root files:

- `Persistence.config.json`
- `Ai.config.json`
- `Azure.config.json`

Module configuration lives under:

```text
src/ProjectName.Host/config/modules/
```

Current module files:

- `Auth.config.json`
- `Users.config.json`

Feature flags live under:

```text
src/ProjectName.Host/config/features/Features.config.json
```

`appsettings.json` should stay focused on host-level ASP.NET Core defaults such as logging and allowed hosts.

Project config files use `Owner.config.json` and `Owner.{Environment}.config.json` names so file tabs and search results remain understandable outside the folder path.

## Adding Module Configuration

Add a module-owned file named after the module:

```text
src/ProjectName.Host/config/modules/Billing.config.json
```

Keep the section rooted by module name:

```json
{
  "Billing": {
    "Invoices": {
      "GracePeriodDays": 7
    }
  }
}
```

Module infrastructure owns binding and validation for its own options. Shared generic binding helpers live in `Gorgeous.Web.Configuration`.

## Validated Options

Use typed options for structural configuration:

```csharp
services.AddValidatedOptions<MyOptions, MyOptionsValidator>(
    configuration,
    MyOptions.SectionName);
```

The helper binds the required section, registers `IValidateOptions<TOptions>`, and enables startup validation. It has no product-specific section names and no Azure-specific behavior.

## Secrets

Do not commit secrets.

The repository intentionally leaves `Auth:Jwt:SigningKey` empty. Set it through user secrets, environment variables, Azure App Configuration with Key Vault references, or the hosting platform secret store.

Example local override:

```powershell
$env:Auth__Jwt__SigningKey = "local-development-signing-key-32-bytes-minimum"
```

The JWT signing key must be at least 32 UTF-8 bytes and must not contain placeholder markers such as `CHANGE_ME`.

## Feature Flags

Feature flags use Microsoft Feature Management 4.x. Local JSON files use an object-shaped schema so environment-specific files and command-line or environment overrides merge by feature name instead of array index:

```json
{
  "FeatureManagement": {
    "PlaceholderScopeA.PlaceholderFeature": false,
    "PlaceholderScopeB.PlaceholderFeature": false
  }
}
```

Feature names live in `Shared.Conventions.AppFeatures`.

Product code should depend on `Shared.AppModel.Abstractions.IFeatureGate`, not on `Microsoft.FeatureManagement`. The host adapts `IFeatureGate` to Microsoft Feature Management.

## Runtime Changes

JSON files and environment variables are startup configuration for normal deployments. Restart the host after changing them.

Azure App Configuration is wired with the ASP.NET Core provider and feature flag support. The current template is ready for Azure-backed configuration and refresh middleware, but product code should still treat configuration changes as operational changes that need testing before rollout.

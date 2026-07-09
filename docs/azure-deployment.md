# Azure Deployment

## Azure Readiness

The host is ready to load centralized configuration from Azure App Configuration without leaking Azure SDK dependencies into modules or application code.

Azure-specific package references are limited to `ProjectName.Host`:

- `Microsoft.Azure.AppConfiguration.AspNetCore`
- `Azure.Identity`
- `Microsoft.FeatureManagement.AspNetCore`

Modules use typed options and `IFeatureGate` abstractions instead.

## Enable Azure App Configuration

Azure App Configuration is disabled by default:

```json
{
  "Azure": {
    "AppConfiguration": {
      "Enabled": false,
      "Endpoint": ""
    }
  }
}
```

Enable it with environment variables or a secure deployment setting:

```powershell
$env:Azure__AppConfiguration__Enabled = "true"
$env:Azure__AppConfiguration__Endpoint = "https://my-app-config.azconfig.io"
```

When `Enabled=false`, the host does not connect to Azure. When `Enabled=true` and `Endpoint` is present, the host connects with `DefaultAzureCredential`.

## Identity And Secrets

Use managed identity in Azure whenever possible. `DefaultAzureCredential` can authenticate through managed identity in deployed environments and developer credentials locally.

Keep secrets out of repository JSON files. Store production secrets in Key Vault and expose them through Azure App Configuration Key Vault references or the hosting platform secret system.

Important secret-backed settings:

- `Auth:Jwt:SigningKey`
- production database connection strings
- future email or AI provider credentials

## Feature Flags

Local JSON feature flags use the `FeatureManagement` object schema so overrides merge by feature name. Azure App Configuration can still own production feature flags through its feature-management integration.

Product code reads flags through `IFeatureGate`; modules should not reference Azure packages or `Microsoft.FeatureManagement`.

## Restart And Refresh

Local JSON and environment variable changes require restart.

Azure App Configuration is registered with the ASP.NET Core provider and middleware. This gives the host the right place to add sentinel-key refresh policies later. Until a product-specific refresh policy is added, use restart-based rollout for structural configuration and treat dynamic feature flag changes as operationally controlled changes.

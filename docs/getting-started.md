# Getting Started

## Requirements

- .NET 10 SDK.
- A local shell that can run `dotnet`.

The application uses SQLite by default and creates a local database through EF Core migrations when the host starts in Development.

## Restore And Build

```powershell
dotnet restore .\ProjectName.slnx
dotnet build .\ProjectName.slnx
```

Run tests:

```powershell
dotnet test .\ProjectName.slnx
```

## Local Configuration

The default connection string is:

```json
"ConnectionStrings": {
  "Default": "Data Source=projectname.db"
}
```

The JWT signing key is intentionally empty in source control. Set it through environment variables or another local secret mechanism before starting the app.

```powershell
$env:Auth__Jwt__SigningKey = "local-development-signing-key-32-bytes-minimum"
```

The signing key must be at least 32 UTF-8 bytes. Placeholder values are rejected on startup.

## Run The Host

```powershell
dotnet run --project .\src\ProjectName.Host\ProjectName.Host.csproj
```

Development launch URLs:

- `http://localhost:5282`
- `https://localhost:7267`

Smoke check:

```text
GET /
```

Expected shape:

```json
{
  "service": "ProjectName.Host"
}
```

## Database Initialization

On startup, the host applies EF Core migrations when either condition is true:

- the environment is Development;
- `Persistence:ApplyMigrationsOnStartup` is set to `true`.

The project uses one SQLite database with two DbContexts:

- `UsersDbContext` owns users, roles, and user-role assignments.
- `AuthDbContext` owns ASP.NET Core Identity tables and refresh sessions.

Each context has its own migrations history table.

## Main HTTP Areas

Authentication endpoints are under:

```text
/api/auth
```

Current-user and user administration endpoints are under:

```text
/api/users
```

Most user endpoints require authentication. Administrative user endpoints require the `admin` role policy.

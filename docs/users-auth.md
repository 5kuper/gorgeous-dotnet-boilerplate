# Users And Auth

`Users` and `Auth` are the reference modules shipped with the project.

They demonstrate module boundaries, module contracts, shared transactions, Minimal API endpoint mapping, Identity integration, JWT issuing, refresh-session rotation, and result-to-ProblemDetails error mapping.

## Ownership

`Users` owns business user state:

- user profile;
- lifecycle status;
- email verification from the business perspective;
- roles and role assignment;
- authentication profile data exposed to other modules.

`Auth` owns authentication workflows:

- email/password registration;
- credential validation through ASP.NET Core Identity;
- access token issuing;
- refresh session creation, rotation, and revocation;
- logout;
- email confirmation orchestration;
- password reset orchestration.

`Auth` can depend on `Users.Contracts`.
`Users` does not depend on `Auth`.

## User Identifiers

The project uses two user identifiers:

- internal numeric `Id` for storage relationships;
- public `Guid PublicId` for APIs, URLs, and public token subject values.

Access tokens use the public id as the public subject. Internal ids can be used internally when the backend needs them.

## Users Module

Important domain types:

- `User`
- `Role`
- `UserRole`
- `UserStatus`
- `RegistrationMethod`

The default role codes are:

- `user`
- `admin`
- `support`

Roles are business roles owned by `Users`. ASP.NET Core Identity roles are not the source of truth for product authorization.

Important contracts:

- `IUsersRegistration`
- `IUsersEmailVerification`
- `IUserAuthProfileReader`
- `UserAuthProfile`

`IUserAuthProfileReader` is a read projection used by `Auth` to build tokens and decide whether a user can authenticate.

## Auth Module

Important domain type:

- `RefreshSession`

Important application ports:

- `IIdentityCredentialService`
- `IRegistrationTransaction`
- `ITokenIssuer`
- `IEmailConfirmationSender`
- `IPasswordResetSender`
- `IEmailConfirmationCodeProtector`

The infrastructure implementation uses:

- ASP.NET Core Identity for credentials;
- EF Core for Identity and refresh-session persistence;
- Data Protection for email confirmation payload protection;
- JWT bearer tokens for access tokens.

Email and password-reset senders are currently no-op implementations. Replace them with real infrastructure before production use.

## Registration Flow

```text
POST /api/auth/register
  -> Auth.Application RegisterWithEmailPassword
  -> IRegistrationTransaction
      -> Users creates the business user and default role assignment
      -> Auth creates the Identity user
  -> commit
  -> Auth sends confirmation message after commit
```

Registration creates the business user and Identity user consistently through a shared EF Core transaction.

If email confirmation is required, delivery happens after the database transaction commits.

## Login Flow

```text
POST /api/auth/login
  -> validate credentials with Identity
  -> read UserAuthProfile from Users
  -> reject users that cannot authenticate
  -> create refresh session
  -> issue access token
```

Access tokens include role claims from `Users`.

User suspension and role changes take effect for new logins and refreshes. Already issued access tokens remain valid until expiration.

## Refresh Flow

```text
POST /api/auth/refresh
  -> hash incoming refresh token
  -> load refresh session
  -> read UserAuthProfile from Users
  -> rotate refresh session
  -> issue new access and refresh tokens
```

Refresh tokens are not stored directly. The system stores token hashes.

Refresh token rotation is transactional. Reuse of a revoked or replaced token is treated as replay and revokes active sessions for the user.

## HTTP Endpoints

Auth endpoints:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
POST /api/auth/confirm-email
POST /api/auth/password-reset/request
POST /api/auth/password-reset/reset
```

Users endpoints:

```text
GET    /api/users/me
PATCH  /api/users/me
POST   /api/users/admin/roles
PUT    /api/users/admin/{publicId}/roles/{roleCode}
DELETE /api/users/admin/{publicId}/roles/{roleCode}
POST   /api/users/admin/{publicId}/suspend
```

## Extension Points

Common next steps:

- replace no-op email senders;
- add production secret storage;
- add external login providers through Identity;
- add MFA through Identity;
- add permissions if roles become too coarse;
- add audit events for authentication and role changes;
- add session management endpoints for users and administrators.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build
```bash
dotnet build screendrafts.api.slnx
```

### Run the API
Start infrastructure first, then run the web project:
```bash
docker compose up -d screendrafts.database screendrafts.identity screendrafts.seq screendrafts.redis screendrafts.jaeger screendrafts.queue screendrafts.mongo screendrafts.email
dotnet run --project src/api/ScreenDrafts.Web/ScreenDrafts.Web.csproj
```

Or start everything via Docker Compose:
```bash
docker compose up
```

### Tests
Run all tests:
```bash
dotnet test screendrafts.api.slnx
```

Run a specific module's unit tests:
```bash
dotnet test src/modules/drafts/ScreenDrafts.Modules.Drafts.UnitTests/ScreenDrafts.Modules.Drafts.UnitTests.csproj
```

Run a specific module's integration tests:
```bash
dotnet test src/modules/drafts/ScreenDrafts.Modules.Drafts.IntegrationTests/ScreenDrafts.Modules.Drafts.IntegrationTests.csproj
```

Run architecture tests:
```bash
dotnet test src/modules/drafts/ScreenDrafts.Modules.Drafts.ArchitectureTests/ScreenDrafts.Modules.Drafts.ArchitectureTests.csproj
```

### EF Core Migrations
Add a migration for a module (e.g., Drafts):
```bash
dotnet ef migrations add <MigrationName> --project src/modules/drafts/ScreenDrafts.Modules.Drafts.Infrastructure --startup-project src/api/ScreenDrafts.Web
```

### Database Seeding
```bash
# Bash
./tools/seed.sh drafts
./tools/seed.sh movies --module=genres

# PowerShell
.\tools\seed.ps1 -Seeder drafts
.\tools\seed.ps1 -Seeder drafts -Module drafts
```

## Architecture

### Modular Monolith
The solution is a .NET 10 modular monolith. Each module is completely self-contained with its own domain, data access, and API surface.

**Modules** (under `src/modules/`):
- `administration` — admin-level configuration
- `audit` — audit trail
- `communications` — email/notifications
- `drafts` — core domain: drafts, draft parts, picks, categories, campaigns, series, drafters, hosts, teams
- `integrations` — third-party integrations
- `movies` — movie catalog (OMDB/IMDb data)
- `real-time-updates` — SignalR/real-time features
- `reporting` — reporting features
- `users` — user accounts (backed by Keycloak)

**API entry point**: `src/api/ScreenDrafts.Web` — references only the `*.Composition` project of each module. `src/api/ScreenDrafts.Gateway` is a YARP reverse proxy.

### Per-Module Project Structure
Every module follows this project layout:

| Project | Purpose |
|---------|---------|
| `.Domain` | Aggregates, entities, value objects, domain events, repository interfaces |
| `.Features` | FastEndpoints, commands/queries, handlers, validators, domain event dispatchers |
| `.Infrastructure` | EF Core `DbContext`, repository implementations, EF migrations, outbox/inbox Quartz jobs |
| `.Composition` | DI registration and module bootstrapping (`AddXxxModule`) |
| `.IntegrationEvents` | Integration event types shared across modules |
| `.PublicApi` | Public API surface for cross-module calls (selected modules) |
| `.UnitTests` | Domain unit tests (xUnit + Bogus + FluentAssertions) |
| `.IntegrationTests` | HTTP-level tests using Testcontainers (PostgreSQL, Redis, RabbitMQ, MongoDB, Keycloak) |
| `.ArchitectureTests` | Layer dependency rules enforced with NetArchTest |

### Feature Slice Structure
Each feature lives in its own folder inside `.Features` and contains:

```
Drafts/CreateDraftPart/
├── Endpoint.cs                      # FastEndpoints endpoint (extends ScreenDraftsEndpoint<TReq, TRes>)
├── CreateDraftPartCommand.cs        # ICommand<TResponse> (MediatR request)
├── CreatedDraftPartCommandHandler.cs # ICommandHandler<TCommand, TResponse>
├── CreateDraftPartRequest.cs        # HTTP request model
├── Validator.cs                     # FluentValidation validator
└── Summary.cs                       # OpenAPI summary
```

- Queries follow the same pattern with `IQuery<TResponse>` and `IQueryHandler<TCommand, TResponse>`.
- Endpoint authorization uses permission strings from the module-internal `*Auth.cs` file (e.g., `DraftsAuth.Permissions.DraftPartCreate`).

### Domain Layer Patterns
- Aggregates extend `AggregateRoot<TId, TIdType>` and use `Raise(domainEvent)` to emit domain events.
- Domain methods return `Result` or `Result<T>` (never throw for business rule violations).
- `SDError` is the error type; static error factories live in `*Errors.cs` classes alongside their aggregate.
- Entities use two IDs: a `Guid` internal DB id and a `string` `PublicId` (Nanoid-based) exposed in the API.
- Repository interfaces live in `.Domain`; implementations live in `.Infrastructure`.

### CQRS & MediatR Pipeline
Commands flow through:
1. `DraftsUnitOfWorkBehavior` — calls `SaveChangesAsync` after every command
2. Command handler (resolves aggregate from repository, mutates, re-saves via the UoW behavior)

Domain events raised inside the aggregate are dispatched after persistence by `DraftsDomainEventDispatcher` (outbox-backed, idempotent via `IdempotentDomainEventHandler<>`).

Integration events flow via MassTransit/RabbitMQ. Each module registers its consumers in `XxxModule.ConfigureConsumers`.

### Infrastructure
- **Database**: PostgreSQL with one schema per module (e.g., `drafts`, `users`). EF Core with `EFCore.NamingConventions` (snake_case). Each module has its own `DbContext`.
- **Read queries**: Dapper via `IDbConnectionFactory` — feature-specific query handlers query the DB directly with SQL.
- **Cache**: Redis (`IDistributedCache`).
- **Event bus**: MassTransit over RabbitMQ.
- **Scheduled jobs**: Quartz.NET (outbox/inbox processors).
- **Auth**: Keycloak JWT. Authorization is permission-based (claims from the token).
- **Logging**: Serilog → Seq (`:8081`). Tracing: OpenTelemetry → Jaeger (`:16686`).

### Configuration
Per-module config is split into separate JSON files in `ScreenDrafts.Web`:
- `modules.drafts.json` / `modules.drafts.Development.json`
- `modules.users.json` / `modules.users.Development.json`
- … (one pair per module)

Sensitive development settings (connection strings, secrets) go in `appsettings.Development.json` or user secrets.

### Build Constraints
- `TreatWarningsAsErrors=true` globally — all compiler warnings break the build.
- `SonarAnalyzer.CSharp` is enabled on all projects. Fix Sonar issues before committing.
- Target framework: `net10.0`. SDK: `10.0.101`.
- Central package management via `Directory.Packages.props` — do not add version attributes to individual `<PackageReference>` elements; add/update versions only in `Directory.Packages.props`.

### Testing Conventions
- **Unit tests**: Extend `DraftsBaseTest` (or the equivalent base for other modules). Use `Faker` (Bogus) for test data. Test domain behavior, not infrastructure.
- **Integration tests**: Extend `IntegrationTestWebAppFactory` (Testcontainers spins up real Postgres/Redis/RabbitMQ/MongoDB/Keycloak). Apply EF migrations via `ApplyMigrationsAsync`.
- **Architecture tests**: Use NetArchTest to enforce that e.g. `.Domain` does not reference `.Infrastructure`.

### Personal Preferences
- Use expression-bodied members where concise.
- Use `var` when the type is obvious from the right-hand side.
- Use pattern matching (`is`, `switch`) instead of null checks or type checks.
- Keep methods short (ideally <20 lines) and focused on a single responsibility.
- Organize using directives in a GlobalUsings.cs file per project. 

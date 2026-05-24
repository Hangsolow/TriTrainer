# Copilot Instructions for TriTrainer

## Project Overview

TriTrainer is a **triathlon training plan manager** built as a **.NET Aspire distributed application**:

- **AppHost** (`TriTrainer.AppHost`) — Aspire orchestration project managing the full distributed system
- **ApiService** (`TriTrainer.ApiService`) — REST API (net10.0) backed by PostgreSQL via EF Core. Manages training activities.
- **Web** (`TriTrainer.Web`) — Blazor Server frontend (net10.0) with an interactive monthly training calendar
- **ServiceDefaults** (`TriTrainer.ServiceDefaults`) — Shared configuration library used by all services

All source projects live in `/src`; test projects in `/tests`.

## Build, Test, and Run

### Build the entire solution
```bash
dotnet build TriTrainer.slnx
```

### Run the distributed application

Use the Aspire CLI (recommended):
```bash
aspire start
```

Or run the AppHost directly:
```bash
dotnet run --project src/TriTrainer.AppHost/TriTrainer.AppHost.csproj
```

> **Note:** Requires a Docker-compatible runtime (Docker Desktop or Podman with Docker compatibility enabled). On Windows with Podman, set `DOCKER_HOST=npipe:////./pipe/docker_engine` before starting.

This starts the Aspire orchestrator, which manages PostgreSQL, ApiService, and the Web frontend. The Aspire dashboard URL is printed to stdout.

### Run unit tests (TUnit)

> **Important:** TUnit test projects have `OutputType: Exe` — run with `dotnet run`, not `dotnet test`.

Run all unit test projects:
```bash
dotnet run --project tests/TriTrainer.ApiService.Tests/TriTrainer.ApiService.Tests.csproj --configuration Release
dotnet run --project tests/TriTrainer.Web.Tests/TriTrainer.Web.Tests.csproj --configuration Release
dotnet run --project tests/TriTrainer.ServiceDefaults.Tests/TriTrainer.ServiceDefaults.Tests.csproj --configuration Release
```

Run integration tests (requires Docker/Podman — starts full Aspire stack):
```bash
dotnet run --project tests/TriTrainer.IntegrationTests/TriTrainer.IntegrationTests.csproj --configuration Release
```

Run with code coverage or TRX report:
```bash
dotnet run --project tests/TriTrainer.ApiService.Tests/TriTrainer.ApiService.Tests.csproj --configuration Release --coverage
dotnet run --project tests/TriTrainer.ApiService.Tests/TriTrainer.ApiService.Tests.csproj --configuration Release --report-trx
```

> Do **not** use coverlet with TUnit — incompatible.

## Architecture & Key Patterns

### Aspire Orchestration
The AppHost uses Aspire's `DistributedApplication` to:
- Start and manage the ApiService and Web services
- Wire up service references (Web → ApiService)
- Configure health checks for each service (`/health` endpoint)
- Enable automatic service discovery

### Service Discovery
Services communicate using the "https+http://" scheme:
- Web calls ApiService via `https+http://apiservice` (see `ActivityApiClient` in Web)
- Service discovery automatically resolves these URLs at runtime
- Configured in `ServiceDefaults.AddServiceDiscovery()`

### ServiceDefaults Pattern
All service projects reference `TriTrainer.ServiceDefaults` and call `builder.AddServiceDefaults()` in Program.cs. This provides:
- **Service Discovery** — Resolves service URLs dynamically
- **Resilience** — Standard resilience handler for HTTP clients
- **Health Checks** — `/health` and `/alive` endpoints
- **OpenTelemetry** — Metrics, tracing, and logging exporters configured
- **Default Endpoints** — Maps health check routes in development

### Health Checks
- `/health` — All checks must pass (app is ready to receive traffic)
- `/alive` — Only "live" tag checks must pass (app is alive but may be recovering)

## Activity Data Model

### Entity: `Activity` (`src/TriTrainer.ApiService/Data/Activity.cs`)
| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `Date` | `DateOnly` | Training date |
| `Type` | `ActivityType` | `Run`, `Cycle`, or `Swim` (stored as string in DB) |
| `DurationMinutes` | `int` | Duration in minutes |
| `Notes` | `string?` | Optional free text |
| `CreatedAt` | `DateTime` | UTC timestamp |

The database is created automatically on startup via `EnsureCreatedAsync`. AppHost registers a PostgreSQL container (`postgres`) with a database named `tritrainerdb` wired to the ApiService.

## API Endpoints

Base URL resolved by Aspire service discovery as `https+http://apiservice`.

| Method | Path | Description |
|---|---|---|
| `GET` | `/activities?year={y}&month={m}` | List activities for a month |
| `POST` | `/activities` | Create an activity |
| `DELETE` | `/activities/{id}` | Delete an activity by ID |

### POST /activities request body
```json
{ "date": "2026-04-15", "type": "Run", "durationMinutes": 45, "notes": "Optional" }
```

See `src/TriTrainer.ApiService/TriTrainer.ApiService.http` for ready-made HTTP request examples.

## Calendar UI

The Web frontend's only page is `Calendar.razor` (routes `/` and `/calendar`):
- Monthly grid view with prev/next month navigation
- Color-coded activity badges per day: 🏃 Run (blue), 🚴 Cycle (green), 🏊 Swim (cyan)
- Click any day cell to open an inline add-activity form
- Hover a badge to reveal a ✕ delete button
- Uses `@rendermode InteractiveServer` for Blazor interactivity

- **File Structure** — Source in `/src`, tests in `/tests`, AI skills in `.agents/skills/`
- **Service Names** — Service names in AppHost (e.g., `"apiservice"`) are used as hostnames in service discovery
- **OpenAPI** — ApiService exposes OpenAPI at `/openapi/v1.json` in Development
- **Razor Components** — Web frontend uses Blazor Server with `@rendermode InteractiveServer` for interactivity
- **HTTP Clients** — Configured with service discovery and resilience by default
- **ActivityType** — Stored as string in PostgreSQL for readability

## MCP Servers

**Aspire MCP Server** — Configured in `.mcp.json` for distributed app diagnostics. Use the Aspire skill to:
- Query app resources and their health status
- View OpenTelemetry logs and traces
- Start, stop, and restart services
- Execute resource commands

**Playwright MCP Server** — Configured in `.mcp.json` for browser automation. Use for:
- Recording and running Playwright tests
- Test generation and debugging

## Project References

- `TriTrainer.ApiService` → `TriTrainer.ServiceDefaults`
- `TriTrainer.Web` → `TriTrainer.ServiceDefaults`
- `TriTrainer.AppHost` → No direct service projects (uses project type detection)
- `TriTrainer.ApiService.Tests` → `TriTrainer.ApiService` (unit tests with EF InMemory)
- `TriTrainer.IntegrationTests` → `TriTrainer.AppHost` (full Aspire stack tests via TUnit.Aspire)

## Sprint Execution Persistence

- When operating in sprint coordination mode, always continue executing the current "Next 24h Focus" items without waiting for repeated user nudges.
- After each lane completes, immediately refresh `docs/sprint-N/progress.md` and generate the next "Next 24h Focus" list, then continue with the next actionable owner.
- Continue this execution loop until the sprint is ready for review/merge (all required merge gates pass and QA sign-off status is merge-ready), unless explicitly blocked or the user instructs to pause/redirect.

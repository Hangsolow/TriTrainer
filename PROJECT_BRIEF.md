# PROJECT_BRIEF.md - TriTrainer

> Last updated: 2026-05-24 | Sprint 5 | Status: Done (Ready to Merge)

## 1. Project Overview

TriTrainer is a triathlon training platform focused on planning training blocks, setting measurable goals, and tracking progress across swim, bike, and run disciplines. The current repository contains a proof of concept that validates core technical plumbing but does not yet represent the target product experience. The goal is to evolve this codebase into a production-ready system with a redesigned UI, expanded backend domain, and clear team workflow for parallel AI execution. Hard technical constraints are a C# backend and Aspire-based local development orchestration.

## 2. Concept / Product Description

TriTrainer helps athletes move from ad-hoc workouts to structured, adaptive triathlon preparation.

Primary user flows:
- Athlete creates profile and training context (race distance, current fitness baseline, available weekly hours).
- Athlete defines goals (event date, finish target, discipline targets, consistency targets).
- System generates and manages training plans organized by macrocycles, mesocycles, and weekly sessions.
- Athlete logs completed sessions and compares planned vs completed volume and intensity.
- Athlete tracks personal records (PRs) and trend improvements by discipline.
- Athlete reviews dashboards for compliance, fatigue indicators, and upcoming plan adjustments.

Key product pillars:
- Plan-first coaching workflow.
- Goal-driven progression tracking.
- PR and milestone visibility.
- Clear, modern UI optimized for daily check-ins.

## 3. Tech Stack

- **Frontend:** Blazor Server (interactive components), modernized design system, reusable component library
- **Backend:** ASP.NET Core Minimal APIs in C#, EF Core, PostgreSQL
- **Hosting:** .NET Aspire AppHost for local orchestration; containerized dependencies (PostgreSQL)
- **Testing:** TUnit for unit/integration testing, Playwright for end-to-end UI verification
- **CI/CD:** GitHub Actions (build, test, quality gates, release pipeline)

## 4. Architecture

```
┌──────────────────────────────────────────────────────┐
│                  TriTrainer.Web (UI)                │
│  Dashboard, Plan Builder, Goals, PR Tracking Views  │
└───────────────────────┬──────────────────────────────┘
                        │ HTTPS via service discovery
┌───────────────────────▼──────────────────────────────┐
│               TriTrainer.ApiService (C#)            │
│  /plans  /goals  /activities  /records  /progress   │
│  Validation, business rules, and aggregation logic  │
└───────────────────────┬──────────────────────────────┘
                        │ EF Core
┌───────────────────────▼──────────────────────────────┐
│                PostgreSQL (tritrainerdb)            │
│  Athletes, Plans, Sessions, Goals, PersonalRecords  │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│             TriTrainer.AppHost (Aspire)             │
│  Orchestrates api, web, postgres, health, telemetry │
└──────────────────────────────────────────────────────┘
```

## 5. Key Files Map

| Area | Path | Contents |
|------|------|----------|
| App orchestration | `src/TriTrainer.AppHost/AppHost.cs` | Aspire resource wiring for web, api, postgres |
| API entry point | `src/TriTrainer.ApiService/Program.cs` | Current minimal API endpoints and startup |
| API domain model | `src/TriTrainer.ApiService/Data/` | Activity entity, type enum, db context |
| Web entry point | `src/TriTrainer.Web/Program.cs` | Blazor app startup and service wiring |
| Current calendar UI | `src/TriTrainer.Web/Components/Pages/Calendar.razor` | Existing PoC calendar workflow |
| Web API client | `src/TriTrainer.Web/ActivityApiClient.cs` | Calls api endpoints via service discovery |
| Solution root | `TriTrainer.slnx` | Solution definition |
| Test projects | `tests/` | Unit and integration tests (TUnit/TUnit.Aspire) |
| Sprint docs | `docs/sprint-1/` | Plan, progress tracking, done handoff |
| Brainstorm artifacts | `docs/brainstorm/` | Ideation, debate, concepts, vote, summary |

## 6. Team Roles

| Agent | Name | Role |
|-------|------|------|
| Producer | Remy | Sprint plans, coordination, issue triage, merge control |
| Frontend | Nova | Blazor component architecture, client interactions, UX delivery |
| Backend | Sage | API contracts, domain model, data integrity, security |
| Art/CSS | Milo | Visual identity, layout system, accessibility polish |
| QA | Ivy | Test strategy, bug filing, regression sign-off |
| Product | Kira | Athlete journeys, feature definition, priority decisions |
| DevOps | Dash | CI/CD, environments, deployment automation |

## 7. Sprint Status

| Sprint | Name | Status | Scope |
|--------|------|--------|-------|
| 0 | PoC Baseline | Done | Existing calendar + activity CRUD proof of concept |
| 1 | Foundation Reboot | Done | Product reframing, architecture updates, redesigned UX baseline, domain and contract expansion, QA sign-off |
| 2 | Plan and Goals Core | Done | Training plan and goals hardening, richer session generation, contract validation, and E2E smoke coverage |
| 3 | Progress and PR Tracking | Done (Conditional Pass) | Personal records, progress analytics, compliance dashboards, CI pipeline wiring, Playwright smoke CI lane, expanded regression coverage |
| 4 | Release Gate Hardening | Done | Closed carry-over CI/Playwright evidence gates under policy, hardened startup-health regression confidence, and completed QA PASS closeout |
| 5 | Athlete Experience Expansion | Done (Ready to Merge) | Delivered recommendation insights, dashboard guidance, quick-start planning flow, and records usability improvements with QA PASS sign-off |

## 8. Current State (rewrite every sprint)

**What works:**
- Aspire AppHost orchestrates web, api, and postgres locally with stable startup-health checks.
- API supports activity CRUD and expanded `/v1` domain contracts for profile, goals, plans, progress, records, and recommendation insights.
- Sprint 5 delivered recommendation guidance end-to-end:
    - `GET /v1/recommendations/insights` returns actionable items from compliance, streak, and next-session signals.
    - Dashboard renders recommendation cards with severity badges and CTA actions.
    - Dashboard fallbacks are hardened for null/empty insights and unknown recommendation metadata.
- Sprint 5 delivered goal/plan quick-start UX and backend reliability:
    - New atomic endpoint `POST /v1/goals/quick-start` creates active goal and starter plan in one operation.
    - Goals UI now supports reduced-friction quick-start defaults and clearer validation/messaging.
- Sprint 5 delivered records usability improvements:
    - Filter/sort summary clarity, reset affordances, empty-state guidance, and row-level `Reuse` quick action.
- CI policy remains stable: GitHub unit-only; integration and Playwright remain local trusted-machine lanes.
- Current validation evidence is green:
    - `dotnet build TriTrainer.slnx` passes.
    - `TriTrainer.Web.Tests` 34/34 pass.
    - `TriTrainer.IntegrationTests` 19/19 pass.
- Sprint 5 QA sign-off artifact exists at `docs/qa/sprint-5-signoff.md` with PASS recommendation.

**What does not work yet:**
- Hosted GitHub runners still cannot reliably execute trusted integration/Playwright lanes under current self-signed certificate trust constraints.
- Recommendation CTA links are page-level and not yet deep-linked with plan/week context.
- Full authentication/authorization and production security model are not implemented.

**What is next:**
- Open and review PR for `feature/sprint-5` with Sprint 5 evidence package.
- Merge Sprint 5 to `main` using regular merge after producer approval.
- Plan next sprint scope (post-Sprint 5) for prioritized product value and any accepted follow-ups (for example CTA deep-linking context).

## 9. Security Rules

1. Secrets live in environment variables or user-secrets only, never committed to source control.
2. Backend remains C# ASP.NET Core; all external-facing business operations flow through validated API endpoints.
3. Add authentication and authorization before production rollout; no anonymous write operations in production mode.
4. Validate all request payloads and enforce business constraints server-side.
5. Use least-privilege database credentials per environment.
6. Keep audit-friendly logs for plan, goal, and record mutations.

## 10. How to Run Locally

```bash
dotnet --info
dotnet restore TriTrainer.slnx
dotnet build TriTrainer.slnx
aspire start
```

Alternative:

```bash
dotnet run --project src/TriTrainer.AppHost/TriTrainer.AppHost.csproj
```

Test commands:

```bash
dotnet run --project tests/TriTrainer.ApiService.Tests/TriTrainer.ApiService.Tests.csproj --configuration Release
dotnet run --project tests/TriTrainer.Web.Tests/TriTrainer.Web.Tests.csproj --configuration Release
dotnet run --project tests/TriTrainer.ServiceDefaults.Tests/TriTrainer.ServiceDefaults.Tests.csproj --configuration Release
dotnet run --project tests/TriTrainer.IntegrationTests/TriTrainer.IntegrationTests.csproj --configuration Release
```

## 11. How to Deploy

Target deployment model:
1. Build and test on pull request via GitHub Actions.
2. Merge to main using regular merge (no squash, no rebase).
3. Build container images for web and api.
4. Publish environment-specific deployment manifests.
5. Apply secrets via environment configuration in deployment platform.
6. Run smoke tests against deployed endpoints.

Immediate action item:
- Run Sprint 5 producer checkpoint cadence and lock day-1 feature acceptance criteria in `docs/sprint-5/progress.md`.

## 12. Cross-Chat Handoff Protocol

Every sprint chat must complete these before ending:

1. Update `docs/sprint-N/progress.md` with latest task status, blockers, and decisions.
2. Update `PROJECT_BRIEF.md` section 7 and fully rewrite section 8.
3. Write `docs/sprint-N/done.md` summarizing built scope, deferred items, and changed files.
4. Commit with descriptive message: `sprint-N: <summary>`.

Cold-start protocol for next chat:
- Prompt: "Read PROJECT_BRIEF.md and docs/sprint-N/progress.md. Continue from where it left off."
- New chat must confirm branch, open blockers, and next unfinished task before writing code.

If these steps are skipped, context continuity is considered broken and no new implementation should start until docs are repaired.

## 13. Bug & Fix Tracking

Bugs are tracked as GitHub Issues and nowhere else as source of truth.

Rules:
- QA files every defect with labels: `bug` + `severity:blocker|major|minor`.
- Every issue includes: component, reproducible steps, expected result, actual result, evidence.
- Dev references issues in commits using `Fixes #NN` when resolved or `Refs #NN` when partial.
- Producer checks open blockers before approving merge.

Artifacts:
- QA writes sprint sign-off in `docs/qa/sprint-N-signoff.md`.
- Backlog ideas that are not defects go to `docs/ideas-backlog.md`.

## 14. Multi-Repo Setup

Each team works in a separate local clone and separate VS Code window.

Recommended clones:
- `tritrainer-producer` on `main` for coordination only.
- `tritrainer-dev` on `feature/sprint-N` for implementation.
- `tritrainer-qa` on `feature/qa-N` for test execution and bug filing.
- `tritrainer-devops` on `feature/devops-N` only when infrastructure work is required.

Branch and merge rules:
- Never push directly to main.
- Use PRs for all changes.
- Use regular merge only (no squash, no rebase).
- Never force push shared branches.

Bootstrap per clone:

```bash
git clone <repo> <folder-name>
cd <folder-name>
git checkout -b <branch-name>
dotnet restore TriTrainer.slnx
```
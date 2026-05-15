# Sprint 1 - Done

## What Was Built
- New planning domain model in API for athlete profile, goals, plans, plan weeks, planned sessions, and personal records.
- New `/v1` minimal API contract surface for profile, goals, plans, weekly progress, and records.
- Dashboard-first frontend navigation and pages: Dashboard, Goals, Plans, Weekly Progress, Records, plus retained Activities page.
- Visual baseline refresh for Sprint 1 (typography, color tokens, card surfaces, improved nav styling).
- Migration safety and domain tests for planning entities with legacy activities preserved.
- QA smoke pass with blocker detection and fix verification.

## What is NOT Done
- Adaptive recommendation engine.
- Advanced analytics dashboards.
- Auth and authorization model.
- Automated end-to-end test suite for new UI pages.

## Files Changed/Created
- `src/TriTrainer.ApiService/Data/PlanningModels.cs` - Sprint 1 planning domain entities and enums.
- `src/TriTrainer.ApiService/Data/ActivitiesDbContext.cs` - EF sets and mappings for planning entities.
- `src/TriTrainer.ApiService/Program.cs` - `/v1` endpoints and DTO-based response mapping.
- `src/TriTrainer.Web/ActivityApiClient.cs` - client models and calls for `/v1` contracts.
- `src/TriTrainer.Web/Components/Pages/Dashboard.razor` - dashboard landing page.
- `src/TriTrainer.Web/Components/Pages/Goals.razor` - goal create/list/status actions.
- `src/TriTrainer.Web/Components/Pages/Plans.razor` - plan create/list/detail flow.
- `src/TriTrainer.Web/Components/Pages/WeeklyProgress.razor` - weekly compliance view.
- `src/TriTrainer.Web/Components/Pages/Records.razor` - personal record create/list.
- `src/TriTrainer.Web/Components/Pages/Calendar.razor` - retained under `/calendar` (no longer root).
- `src/TriTrainer.Web/Components/Layout/NavMenu.razor` - new nav structure.
- `src/TriTrainer.Web/Components/Layout/MainLayout.razor` - updated top-row content.
- `src/TriTrainer.Web/Components/Layout/NavMenu.razor.css` - refreshed nav styling.
- `src/TriTrainer.Web/Components/Layout/MainLayout.razor.css` - updated layout styling.
- `src/TriTrainer.Web/wwwroot/app.css` - global visual system updates.
- `tests/TriTrainer.ApiService.Tests/PlanningDomainTests.cs` - migration safety and aggregation coverage.
- `docs/sprint-1/producer-kickoff.md` - producer kickoff packet.
- `docs/sprint-1/dev-kickoff.md` - dev kickoff packet.
- `docs/sprint-1/progress.md` - sprint progress and bug tracking.
- `docs/qa/sprint-1-signoff.md` - QA pass evidence.

## Manual Setup Required
- Run Aspire stack from repo root: `aspire start`.
- Ensure Docker-compatible runtime is running for postgres/pgadmin resources.

## Known Issues
- No open blockers after final QA pass.
- Non-blocking observation: pgadmin health may flap during immediate restart warm-up; web/api/postgres health remained green for sprint validation.

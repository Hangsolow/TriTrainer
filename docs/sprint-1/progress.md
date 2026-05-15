# Sprint 1 - Progress Tracker

> If context overflows, start a new chat:
> "Read PROJECT_BRIEF.md and docs/sprint-1/progress.md.
> Continue from where it left off."

## Task Status

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | Domain model blueprint | Done | Implemented in API data model with EF mappings |
| 2 | API contract draft | Done | /v1 endpoints implemented for athlete, goals, plans, progress, records |
| 3 | UI information architecture | Done | IA flow documented in dev kickoff packet |
| 4 | Visual language baseline | Done | New app-level visual tokens, typography, and surface/card system added |
| 5 | Vertical slice implementation | Done | Dashboard, Goals, Plans, Weekly Progress, and Records pages wired to /v1 APIs |
| 6 | Migration safety tests | Done | API service tests green and coexistence between legacy activities and new planning entities verified |
| 7 | Regression and sign-off prep | Done | Browser smoke tests completed and QA sign-off recorded as PASS |

## Bugs Found

| # | Description | Severity | Status | Fix |
|---|-------------|----------|--------|-----|
| 1 | POST `/v1/goals` failed with HTTP 500 due JSON cycle serialization of EF navigation graph | blocker | fixed | API changed to return flat DTO responses instead of tracked entities |

## Notes

- Sprint initialized from orchestration docs on 2026-05-15.
- Existing codebase is a PoC baseline and will be iteratively replaced by product-focused flows.
- Hard constraints confirmed: C# backend and Aspire for local orchestration.
- Producer kickoff completed; coordination packet saved in `docs/sprint-1/producer-kickoff.md`.
- Next action: start dev execution on tasks 1-3 with a locked vertical slice and contract-first workflow.
- Dev kickoff completed; implementation-ready packet saved in `docs/sprint-1/dev-kickoff.md`.
- Blocking decisions resolved in implementation:
	- Sprint 1 identity scope uses a single local athlete profile.
	- Weekly progress accepts explicit `weekStart` query date.
	- Plan creation auto-generates week rows only; session rows remain optional.
- Solution build passes after changes.
- Web vertical slice build verified after new pages and navigation integration.
- `TriTrainer.ApiService.Tests` now includes planning domain tests and current run is green (9 passed, 0 failed).
- Aspire lifecycle validation completed per QA flow: stopped running instance, rebuilt solution, restarted resources, then re-ran smoke tests.
- QA sign-off updated at `docs/qa/sprint-1-signoff.md` with PASS result.

# Sprint 2 - Progress Tracker

> If context overflows, start a new chat:
> "Read PROJECT_BRIEF.md and docs/sprint-2/progress.md.
> Continue from where it left off."

## Task Status

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | Plan generation rule set v1 | ✅ Done | `PlanGenerationService.cs` — 3 templates, 4-week mesocycle |
| 2 | Goal/plan validation hardening | ✅ Done | TargetValue guard + `PATCH /v1/plans/{id}/status` + terminal-goal plan creation rejection |
| 3 | Plans UX refinement | ✅ Done | Session pills, status badges, Activate button |
| 4 | Progress UX improvements | ✅ Done | Variance col, compliance colours, zero-row filter |
| 5 | API regression coverage expansion | ✅ Done | Deterministic generation + validation semantics covered; latest run: 137 API tests pass, 0 skipped |
| 6 | Playwright smoke suite v1 | ⏸ Waived | Formal producer waiver approved 2026-05-15 — suite implemented and compiles; execution deferred to Sprint 3 CI pipeline |
| 7 | Sprint hardening and handoff | 🔄 In progress | PR open, awaiting producer merge review |

## Decisions

| Decision | Rationale |
|----------|-----------|
| `PlanGenerationService` is a pure static class | No DI, fully deterministic — easy to unit-test and swap later |
| Sessions attached to `week.Sessions` pre-persist | EF cascade-inserts all in one `SaveChangesAsync()` — no extra round-trips |
| `PlanWeek.Id` uses `init` with `Guid.NewGuid()` | IDs exist at construction, so generator can reference them before DB write |
| `RoundToFive()` with 15-min floor | Avoids sub-5-min session artifacts at low weekly hours |
| PATCH returns 409 for invalid transitions (not 400) | Conflict is a state issue, not a request format issue |
| TargetValue validated server-side only | Frontend sends it; guard is the single authoritative check |

## Changed Files

| File | Change |
|------|--------|
| `src/TriTrainer.ApiService/Services/PlanGenerationService.cs` | **New** — plan generation service (Tasks 1) |
| `src/TriTrainer.ApiService/Program.cs` | TargetValue validation, enum JSON converter, generation call in POST /plans, terminal-goal guard, new PATCH endpoint (Tasks 1+2) |
| `src/TriTrainer.Web/ActivityApiClient.cs` | `UpdatePlanStatusRequest` record + `UpdatePlanStatusAsync` method (Task 2) |
| `src/TriTrainer.Web/Components/Pages/Plans.razor` | Full rewrite — session pills, status badges, Activate button (Task 3) |
| `src/TriTrainer.Web/Components/Pages/WeeklyProgress.razor` | Full rewrite — Variance col, compliance colours, zero-row filter (Task 4) |
| `src/TriTrainer.Web/wwwroot/app.css` | ~80 lines added — session pill + status badge + compliance/variance CSS (Tasks 3+4) |
| `tests/TriTrainer.ApiService.Tests/GenerationAndValidationTests.cs` | **New** — 12 deterministic generation + transition tests (Task 5) |
| `tests/TriTrainer.ApiService.Tests/Sprint2RegressionTests.cs` | `SeedPlanAsync` gains `generateSessions` param; 4 `[Skip]` removed; PATCH stub → parameterised rule test (Task 5) |
| `tests/TriTrainer.ApiService.Tests/QaAdditionalCoverageTests.cs` | Added terminal-goal rejection coverage and extended QA hardening checks |
| `docs/qa/sprint-2-signoff.md` | Updated after blocker fix verification |

## Test Summary (post-Sprint 2 dev lane)

| Suite | Total | Passed | Skipped | Failed |
|-------|-------|--------|---------|--------|
| TriTrainer.ApiService.Tests | 137 | 137 | 0 | 0 |
| TriTrainer.IntegrationTests | 5 | 5 | 0 | 0 |
| TriTrainer.Web.Tests | 27 | 27 | 0 | 0 |
| TriTrainer.ServiceDefaults.Tests | 27 | 27 | 0 | 0 |
| **Total** | **196** | **196** | **0** | **0** |

## Bugs Found

| # | Description | Severity | Status | Fix |
|---|-------------|----------|--------|-----|
| 1 | Razor `@{}` inside `else` block (RZ1010) | Low | Fixed | Used bare local variable declarations in WeeklyProgress.razor |

## Blockers

None open. All dev-lane blockers resolved.

## Notes

- Sprint 2 started after Sprint 1 pass sign-off and done handoff.
- Sprint 2 focus is hardening and validation, not net-new broad surface area.
- Preserve Sprint 1 behavior while extending plan-generation and QA automation depth.
- Producer kickoff package created in `docs/sprint-2/producer-kickoff.md` with day-1 checklist, parallel lane prompts, risk register, and merge gates.
- 2026-05-15 dev checkpoint: executed Tasks 1-4 scope verification and Task 5 support verification via test runs.
- Verified deterministic generation and validation semantics remain stable with current code.
- 2026-05-15 blocker fix: added string-enum JSON deserialization for activity payloads; integration suite now passes 5/5.
- 2026-05-15 hardening follow-up: `POST /v1/plans` now rejects Achieved and Archived goals.
- No blocker defects identified in dev lane.
- All Tasks 1-5 complete. Handoff ready for QA lane and producer merge review.
- Task 6 (Playwright) formal producer waiver approved 2026-05-15. Suite is implemented and compiles (19 smoke tests across 5 flows). Execution deferred to Sprint 3 CI pipeline setup. Waiver recorded in docs/sprint-2/progress.md and docs/qa/sprint-2-signoff.md.

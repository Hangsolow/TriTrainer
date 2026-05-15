# Sprint 2 — Done Handoff

## What Was Built

### Task 1 — Plan Generation Rule Set v1
- `PlanGenerationService` — pure static class, fully deterministic
- Three goal-type templates:
  - **EventFinish**: Run 35% / Cycle 40% / Swim 25%
  - **Consistency**: equal split across all three disciplines
  - **DisciplinePerformance**: focus primary 50% / secondary 20% / cross-train 30%
- 4-week mesocycle loading: base 100%, build 105%, peak 110%, recovery 70%
- Session types by phase: Endurance (base/build), Tempo/Intervals (peak), Recovery (recovery week)
- Duration rounded to nearest 5 min, minimum 15 min per session
- `POST /v1/plans` now generates and cascade-inserts all `PlannedSession` rows

### Task 2 — Validation Hardening
- `POST /v1/goals`: `TargetValue` required and `> 0` for `DisciplinePerformance` goals
- `POST /v1/plans`: rejects Achieved or Archived goals with conflict semantics
- New `PATCH /v1/plans/{planId}/status` endpoint:
  - Draft → Active: 200 OK
  - Active → Completed: 200 OK
  - All other transitions: 409 Conflict

### Task 3 — Plans UX Refinement
- Session pills per week: discipline emoji + session type badge + day + duration
- Week summary: "N sessions · X min planned"
- Plan summary: "N weeks · N sessions · X min planned"
- Colored status badges (Draft / Active / Completed)
- Activate button for Draft plans

### Task 4 — Progress UX Improvements
- Variance column (completed − planned, formatted ±N min)
- Compliance column color-coded: ≥90% green, ≥70% yellow, <70% red
- Zero-zero rows filtered from discipline table
- Overall compliance badge in section header

### Task 5 — Test Coverage
- `GenerationAndValidationTests.cs`: 12 new tests covering generation determinism, all templates, mesocycle, cross-train, and PATCH transition rule table
- `Sprint2RegressionTests.cs`: `SeedPlanAsync` updated; 4 previously-skipped tests un-skipped; PATCH stub replaced with parameterised rule test
- `QaAdditionalCoverageTests.cs`: extended QA coverage, including terminal-goal rejection
- **196 tests total, 0 failed, 0 skipped** across API, integration, web, and service-defaults suites

## What is NOT Done

- **Task 6** (Playwright smoke suite v1): **formal producer waiver approved 2026-05-15**. Suite is fully implemented (19 smoke tests across Dashboard, Goals, Plans, Progress, Records, and legacy flows) and compiles clean. Execution deferred to Sprint 3 when CI pipeline and Docker runtime are in place.
- Task 7 (sprint hardening): closed. All documentation, defect, and handoff artifacts complete.

## Files Changed/Created

| File | Change |
|------|--------|
| `src/TriTrainer.ApiService/Services/PlanGenerationService.cs` | **New** — generation service |
| `src/TriTrainer.ApiService/Program.cs` | TargetValue guard, enum JSON converter, generation wiring, terminal-goal guard, PATCH endpoint |
| `src/TriTrainer.Web/ActivityApiClient.cs` | `UpdatePlanStatusRequest` + `UpdatePlanStatusAsync` |
| `src/TriTrainer.Web/Components/Pages/Plans.razor` | Full rewrite |
| `src/TriTrainer.Web/Components/Pages/WeeklyProgress.razor` | Full rewrite |
| `src/TriTrainer.Web/wwwroot/app.css` | ~80 lines added |
| `tests/TriTrainer.ApiService.Tests/GenerationAndValidationTests.cs` | **New** — 12 tests |
| `tests/TriTrainer.ApiService.Tests/Sprint2RegressionTests.cs` | Updated helper + un-skipped tests |
| `tests/TriTrainer.ApiService.Tests/QaAdditionalCoverageTests.cs` | Additional hardening coverage |
| `docs/sprint-2/progress.md` | Updated with full sprint-2 status |
| `docs/sprint-2/done.md` | This file |

## Manual Setup Required

None. The Aspire AppHost starts PostgreSQL automatically. Run the app with `aspire start` or `dotnet run --project src/TriTrainer.AppHost`.

## Known Issues

Two minor GitHub issues carry forward to Sprint 3 with QA acceptance notes in `docs/qa/sprint-2-signoff.md`:
- **Issue #2** (minor): TUnit version skew — `ApiService.Tests` pins v1.41.0, others use v1.* (v1.44.39). All tests pass today; risk is future divergence.
- **Issue #3** (minor): 20x `TUnit0015` warnings — missing `CancellationToken` on `[Timeout]` tests in `CoreSmokeTests.cs`. Tests run to completion; mitigated by CI global timeout.

No blockers. No major issues. One Razor syntax bug (RZ1010 `@{}` inside `else`) was found and fixed during Task 4.

## QA Handoff Notes

1. **Plan generation is deterministic** — same athlete/goal inputs always produce the same sessions. `GenerationAndValidationTests.cs` documents the exact counts and distributions per template.
2. **PATCH /v1/plans/{id}/status** — test valid transitions (200) and invalid transitions (409). Full transition table is covered by `Sprint2RegressionTests.cs#PlanStatus_TransitionRuleTable_MatchesEndpointImplementation`.
3. **POST /v1/plans** now rejects Achieved and Archived goals with conflict semantics.
4. **DisciplinePerformance goal with TargetValue ≤ 0 or missing** → `POST /v1/goals` must return 400.
5. **Plans page Activate button** — create a plan, confirm badge shows "Draft", click Activate, confirm badge updates to "Active" without page reload.
6. **Progress page zero-row filtering** — a discipline row with 0 planned AND 0 completed must not appear in the table.
7. **Sprint 1 regression** — `/activities` calendar, CRUD, and all Sprint 1 endpoints remain valid; integration suite now verifies create/list/delete again.

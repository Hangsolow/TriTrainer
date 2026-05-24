# Sprint 6 - Progress Tracker

> If context overflows, start a new chat:
> "Read PROJECT_BRIEF.md and docs/sprint-6/progress.md.
> Continue from where it left off."

## Task Status

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | Contextual recommendation navigation | ✅ Done | Recommendation CTAs now deep-link progress recommendations to plan/week context when available with safe fallback behavior |
| 2 | Dashboard readability and hierarchy pass | ✅ Done | Reordered dashboard priority cards, added KPI summary strip, stat-list semantics, and hierarchy-focused section notes while preserving existing recommendation CTA routing behavior |
| 3 | Records workflow speed pass | ✅ Done | Records form now supports quick discipline/date presets, tab-aware discipline defaults, keyboard submit via form submit, and clearer action affordances while preserving existing record contracts |
| 4 | Daily check-in quick path | ⬜ Not started | |
| 5 | QA package and sprint handoff | 🔄 In progress | Automated QA checkpoint completed green; manual UX matrix and final sign-off artifact pending |

## Decisions

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05-24 | Sprint 6 set as UX-focused sprint | User-directed focus on UX improvements |
| 2026-05-24 | Added contextual recommendation href helper instead of replacing existing helper API | Preserve backwards compatibility while enabling plan/week deep-linking on Dashboard |
| 2026-05-24 | Task 2 dashboard hierarchy pass keeps recommendation CTA helper contract intact | Preserve Sprint 6 Task 1 behavior while improving information scanability |

## Bugs Found

_None yet._

## Blockers

_None._

## Notes

- Sprint 6 starts after Sprint 5 merge completion.
- Sprint scope is explicitly UX-first and athlete-facing.
- Any non-UX scope additions should be moved to `docs/ideas-backlog.md`.
- Validation completed for Task 1: `dotnet build TriTrainer.slnx` succeeded; `TriTrainer.Web.Tests` passed (38/38); `TriTrainer.IntegrationTests` passed (19/19).
- Validation completed for Task 2: `dotnet build TriTrainer.slnx` succeeded; `TriTrainer.Web.Tests` passed (41/41); `TriTrainer.IntegrationTests` passed (19/19); `TriTrainer.PlaywrightTests` passed (20/20).
- Validation completed for Task 3: `dotnet build TriTrainer.slnx` succeeded; `TriTrainer.Web.Tests` passed (43/43); `TriTrainer.IntegrationTests` passed (19/19); `TriTrainer.PlaywrightTests` passed (20/20).
- QA acceptance matrix initialized in `docs/qa/sprint-6-acceptance-matrix.md`.
- 2026-05-24 QA gate unblock: Playwright smoke test `Smoke_Navigation_AllPrimaryRoutesReturnSuccess` was instrumented with per-route diagnostics (status, final URL, title, page/console error counts and first error). Failure was isolated to `/progress` returning HTTP 500. Applied minimal hardening in `WeeklyProgress.razor` initialization to catch startup API exceptions and surface an in-page error state instead of route crash. Re-run result: `TriTrainer.PlaywrightTests` passed 20/20. Blocker status: resolved (no owner/ETA needed).

## Risks

| Risk | Impact | Owner | ETA |
|---|---|---|---|
| Dashboard hierarchy updates are currently validated by helper/unit smoke coverage but not by dedicated screenshot-diff assertions | Low | QA (Ivy) + Dev (Nova/Milo) | 2026-05-25 |
| Integration teardown shows intermittent unobserved aborted-request exception noise despite green outcomes | Low | Dev + QA | 2026-05-25 |

## Producer Checkpoint Template

### Status

Status: on-track

### Task Delta

- Task 1: Completed. Dashboard recommendation CTAs now deep-link progress-focused guidance to contextual `planId` and `weekStart` when available, with safe fallback routes.
- Task 2: Completed and QA-cleared. Dashboard hierarchy/readability pass shipped with KPI strip, priority ordering, and semantic stat layout.
- Task 3: Completed and QA-cleared. Records workflows now provide quick presets, tab-aware defaults, and faster keyboard-friendly entry paths.
- Task 4: Not started.
- Task 5: In progress. Automated QA checkpoint remains green; manual UX matrix and final sign-off remain.

### New Risks

| Risk | Impact | Owner | Mitigation |
|---|---|---|---|
| Manual UX validation for deep-link navigation and dashboard hierarchy is not yet fully completed | Medium | QA | Execute focused manual matrix (back/forward, mobile viewport, keyboard flow) and log severity-labeled defects if found |
| Integration teardown emits intermittent aborted-request exception noise despite passing assertions | Low | Dev + QA | Monitor next reruns; create defect if behavior becomes deterministic or user-visible |

### Merge Readiness

- Gate 1: PASS - Sprint 6 scope and task ownership are defined and active.
- Gate 2: PASS - Task 1, Task 2, and Task 3 acceptance criteria implemented and validated.
- Gate 3: PASS - Regression validation green (`build`, Web tests 43/43, Integration tests 19/19, Playwright tests 20/20).
- Gate 4: PASS - CI policy unchanged (GitHub unit-only lane remains baseline).
- Gate 5: PASS - No blocker defects identified in the post-fix QA checkpoint.
- Gate 6: HOLD - Sprint 6 QA sign-off artifact pending sprint completion.
- Gate 7: PASS - Sprint 6 plan, kickoff, progress, and brief alignment are in place.
- Gate 8: PASS - Merge policy remains regular merge only.

### Next 24h Focus

1. Start Task 4 implementation: reduced-friction daily check-in path (Dev).
2. Complete manual UX matrix items and capture evidence in the acceptance matrix (QA).
3. Draft `docs/qa/sprint-6-signoff.md` and prep sprint closeout handoff after Task 4 validation (QA + Producer).

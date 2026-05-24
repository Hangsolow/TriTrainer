# Sprint 6 - Progress Tracker

> If context overflows, start a new chat:
> "Read PROJECT_BRIEF.md and docs/sprint-6/progress.md.
> Continue from where it left off."

## Task Status

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | Contextual recommendation navigation | ✅ Done | Recommendation CTAs now deep-link progress recommendations to plan/week context when available with safe fallback behavior |
| 2 | Dashboard readability and hierarchy pass | ⬜ Not started | |
| 3 | Records workflow speed pass | ⬜ Not started | |
| 4 | Daily check-in quick path | ⬜ Not started | |
| 5 | QA package and sprint handoff | ⬜ Not started | |

## Decisions

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05-24 | Sprint 6 set as UX-focused sprint | User-directed focus on UX improvements |
| 2026-05-24 | Added contextual recommendation href helper instead of replacing existing helper API | Preserve backwards compatibility while enabling plan/week deep-linking on Dashboard |

## Bugs Found

_None yet._

## Blockers

_None yet._

## Notes

- Sprint 6 starts after Sprint 5 merge completion.
- Sprint scope is explicitly UX-first and athlete-facing.
- Any non-UX scope additions should be moved to `docs/ideas-backlog.md`.
- Validation completed for Task 1: `dotnet build TriTrainer.slnx` succeeded; `TriTrainer.Web.Tests` passed (38/38); `TriTrainer.IntegrationTests` passed (19/19).
- QA acceptance matrix initialized in `docs/qa/sprint-6-acceptance-matrix.md`.

## Producer Checkpoint Template

### Status

Status: on-track

### Task Delta

- Task 1: Completed. Dashboard recommendation CTAs now deep-link progress-focused guidance to contextual `planId` and `weekStart` when available, with safe fallback routes.
- Task 2: Not started.
- Task 3: Not started.
- Task 4: Not started.
- Task 5: In progress. QA acceptance matrix initialized for Sprint 6 validation workflow.

### New Risks

| Risk | Impact | Owner | Mitigation |
|---|---|---|---|
| Manual deep-link UX behavior (back/forward, mobile viewport, keyboard flow) is not yet fully validated | Medium | QA | Execute focused manual playtest matrix and file severity-labeled defects if found |
| Integration lane startup sensitivity can add noise to timing/reproducibility | Low | Dev + QA | Continue collecting rerun evidence and monitor for repeatability issues |

### Merge Readiness

- Gate 1: PASS - Sprint 6 scope and task ownership are defined and active.
- Gate 2: PASS - Task 1 acceptance criteria implemented for contextual recommendation navigation.
- Gate 3: PASS - Regression validation green (`build`, Web tests 38/38, Integration tests 19/19).
- Gate 4: PASS - CI policy unchanged (GitHub unit-only lane remains baseline).
- Gate 5: PASS - No blocker defects identified in Task 1 checkpoint.
- Gate 6: HOLD - Sprint 6 QA sign-off artifact pending sprint completion.
- Gate 7: PASS - Sprint 6 plan, kickoff, progress, and brief alignment are in place.
- Gate 8: PASS - Merge policy remains regular merge only.

### Next 24h Focus

1. Start Task 2 implementation: dashboard readability and hierarchy pass (Dev).
2. Run QA manual deep-link UX matrix for Task 1 (QA).
3. Prepare Task 3 records workflow UX change list and acceptance criteria (Producer + Dev + QA).

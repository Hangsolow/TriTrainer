# Sprint 3 - Progress and PR Tracking

> Sprint Goal: Make the athlete's progress and personal records visible, measurable, and trustworthy. Stand up CI so every future commit is automatically verified.
> Branch: feature/sprint-3
> Estimated effort: 1.5-2 weeks

## Prioritized Task List

| # | Task | Owner | Est | Description |
|---|------|-------|-----|-------------|
| 1 | CI pipeline v1 | Dash | 4h | GitHub Actions workflow: build, unit tests, integration tests, artifact publish |
| 2 | Playwright smoke execution | Ivy + Dash | 3h | Wire existing 19-test smoke suite into CI; confirm all paths pass against Aspire stack |
| 3 | Personal Records API hardening | Sage | 5h | Tighten `/v1/records` validation, add GET by discipline, add personal-best query, resolve carried-forward minor issues #2 and #3 |
| 4 | Progress analytics API | Sage | 5h | Aggregate compliance trend over N weeks; expose weekly streak and longest streak via `/v1/progress/summary` |
| 5 | Records UX | Nova + Milo | 5h | Records page: discipline tabs, personal-best highlight, trend sparkline, PR badge on new record |
| 6 | Compliance dashboard UX | Nova + Kira | 5h | Dashboard: rolling 4-week compliance chart, streak widget, next planned session card |
| 7 | API regression coverage expansion | Ivy + Sage | 5h | Tests for records hardening, progress summary aggregation, streak logic, and CI green-gate verification |
| 8 | Sprint hardening and handoff | Remy + Ivy | 3h | QA sign-off, done.md, PROJECT_BRIEF.md update, producer merge review |

## Work Schedule

### Phase 1: CI and smoke execution (Tasks 1-2)
- Unblock all future sprints by making CI the source of truth for test green.
- Confirm Playwright smoke suite passes end-to-end before any new feature code lands.
- Checkpoint commit after phase.

### Phase 2: Backend expansion (Tasks 3-4)
- Harden records API and add progress summary and streak endpoints.
- Fix carried-forward minor issues #2 and #3 in this phase.
- Checkpoint commit after phase.

### Phase 3: Frontend features (Tasks 5-6)
- Build Records page and compliance dashboard widgets over the new API contracts.
- Checkpoint commit after phase.

### Phase 4: QA and handoff (Tasks 7-8)
- Expand regression coverage, confirm CI green, close sprint.
- Final commit and PR.

## Success Criteria

- [ ] GitHub Actions CI runs on every PR: build + unit + integration tests pass.
- [ ] Playwright smoke suite (19 tests) passes in CI against live Aspire stack.
- [ ] `/v1/records` GET by discipline and personal-best query return correct data.
- [ ] `/v1/progress/summary` returns rolling compliance trend and streak counts.
- [ ] Records page shows discipline tabs, personal-best highlight, and trend sparkline.
- [ ] Dashboard shows 4-week compliance chart and streak widget.
- [ ] TUnit version skew (issue #2) and TUnit0015 warnings (issue #3) resolved.
- [ ] QA sign-off is PASS with no open blockers.

## What is NOT in This Sprint

| Feature | Reason |
|---------|--------|
| Adaptive recommendation engine | Deferred to Sprint 4+ — requires more data history |
| Auth / authorization rollout | Dedicated security sprint required |
| Coach mode / multi-athlete | Product sprint required — backlog |
| Race simulation planner | Backlog — needs taper algorithm design |
| Blazor Interactive Auto render mode | Backlog — not blocking current flows |

## Agent Prompt

> Read PROJECT_BRIEF.md, then read docs/sprint-3/plan.md. Execute Sprint 3.
>
> First: git pull origin main && git checkout -b feature/sprint-3
>
> Close GitHub Issues in commits: "fix: description (Fixes #NN)"
> Update docs/sprint-3/progress.md after each phase.
> When done, push and create PR: git push origin feature/sprint-3
> Follow Sections 12-14 of PROJECT_BRIEF.md.

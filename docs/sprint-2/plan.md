# Sprint 2 - Plan and Goals Core

> Sprint Goal: Harden the Sprint 1 vertical slice by improving plan generation, strengthening API validation, and adding end-to-end smoke coverage.
> Branch: feature/sprint-2
> Estimated effort: 1.5-2 weeks

## Prioritized Task List

| # | Task | Owner | Est | Description |
|---|------|-------|-----|-------------|
| 1 | Plan generation rule set v1 | Sage | 6h | Add discipline templates and weekly defaults for auto-generated sessions |
| 2 | Goal/plan validation hardening | Sage | 4h | Tighten status transitions and request validation semantics on `/v1` endpoints |
| 3 | Plans UX refinement | Nova + Kira | 6h | Improve plan creation/detail UX for generated sessions and clearer user feedback |
| 4 | Progress UX improvements | Nova + Milo | 4h | Improve weekly progress readability (variance/compliance emphasis) |
| 5 | API regression coverage expansion | Ivy + Sage | 6h | Add tests for generation rules, status transitions, and invalid payload behavior |
| 6 | Playwright smoke suite v1 | Ivy + Nova | 6h | Implement core E2E smoke path for Dashboard -> Goals -> Plans -> Progress -> Records |
| 7 | Sprint hardening and handoff | Remy + Ivy | 3h | Final QA sign-off and sprint handoff updates |

## Work Schedule

### Phase 1: Backend hardening (tasks 1-2)
- Implement generation and validation rules.
- Checkpoint commit after phase.

### Phase 2: Frontend refinement (tasks 3-4)
- Improve plan and progress workflows around generated content.
- Checkpoint commit after phase.

### Phase 3: QA and automation (tasks 5-7)
- Expand tests and run E2E smoke validation.
- Final commit and PR.

## Success Criteria

- [ ] Plan creation generates meaningful discipline-specific session scaffolds.
- [ ] Goal and plan validation rules prevent invalid transitions and malformed requests.
- [ ] Plans and weekly progress pages show clearer generated-session context.
- [ ] API regression tests cover major success and failure paths for Sprint 2 rules.
- [ ] Playwright smoke suite runs the core vertical path with no blockers.
- [ ] QA sign-off is PASS with no open blockers.

## What is NOT in This Sprint

| Feature | Reason |
|---------|--------|
| Adaptive recommendation engine | Deferred to Sprint 3+ due algorithm and data complexity |
| Full auth/authorization implementation | Requires dedicated security sprint planning |
| Advanced analytics dashboards | Depends on additional telemetry/read model work |

## Agent Prompt

> Read PROJECT_BRIEF.md, then read docs/sprint-2/plan.md. Execute Sprint 2.
>
> First: git pull origin main && git checkout -b feature/sprint-2
>
> Close GitHub Issues in commits: "fix: description (Fixes #NN)"
> Update docs/sprint-2/progress.md after each phase.
> When done, push and create PR: git push origin feature/sprint-2
> Follow Sections 12-14 of PROJECT_BRIEF.md.

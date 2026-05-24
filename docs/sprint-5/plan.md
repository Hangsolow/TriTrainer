# Sprint 5 - Athlete Experience Expansion

> Sprint Goal: Ship visible athlete-facing product value by adding recommendation insights, faster goal/planning flows, and stronger daily usability in dashboard and records workflows.
> Branch: feature/sprint-5
> Estimated effort: 1-1.5 weeks

## Prioritized Task List

| # | Task | Owner | Est | Description |
|---|------|-------|-----|-------------|
| 1 | Recommendation insights API MVP | Sage | 6h | Add backend recommendation endpoint(s) driven by recent compliance/streak/plan signals for actionable athlete guidance |
| 2 | Dashboard recommendation widgets | Nova + Milo | 6h | Add recommendation cards and CTA flows on dashboard using new API contracts |
| 3 | Goal and plan quick-start improvements | Nova + Sage | 5h | Improve goal-to-plan creation flow with reduced friction, clearer defaults, and stronger validation feedback |
| 4 | Records usability improvements | Nova + Milo | 4h | Improve records interaction quality (sorting/filter clarity, empty states, and quick actions) |
| 5 | QA package and sprint handoff | Ivy + Remy | 4h | Publish Sprint 5 QA sign-off and complete done/progress/brief handoff artifacts |

## Work Schedule

### Phase 1: Backend and Contracts (Tasks 1-2)
- Land recommendation contracts first, then wire dashboard feature UX.
- Keep changes scoped to sprint feature outcomes.
- Checkpoint commit after phase.

### Phase 2: Workflow UX Improvements (Tasks 3-4)
- Improve goal/plan and records flows for daily use.
- Avoid infra/policy scope drift.
- Checkpoint commit after phase.

### Phase 3: QA and Producer Closeout (Task 5)
- Validate product behavior and regression safety.
- Publish sign-off and closeout package.

## Success Criteria

- [ ] Recommendation API returns actionable guidance from athlete progress signals.
- [ ] Dashboard shows recommendation widgets with clear athlete actions.
- [ ] Goal-to-plan quick-start flow is faster and less error-prone.
- [ ] Records page usability improvements are shipped and validated.
- [ ] QA sign-off artifact exists with PASS or CONDITIONAL PASS and explicit blocker status.

## What is NOT in This Sprint

| Feature | Reason |
|---------|--------|
| Hosted CI cert-trust remediation | Deferred to dedicated ops/infra sprint |
| Full auth/authorization rollout | Separate security sprint required |
| Coach mode / multi-athlete workflows | Product expansion outside current sprint objective |
| Race simulation / taper planning | Backlog item for later planning sprint |
| Large cross-app redesign | Keep scope focused on high-impact incremental UX |

## Agent Prompt

> Read PROJECT_BRIEF.md, then read docs/sprint-5/plan.md. Execute Sprint 5.
>
> First: git pull origin main && git checkout -b feature/sprint-5
>
> Close GitHub Issues in commits: "fix: description (Fixes #NN)"
> Update docs/sprint-5/progress.md after each phase.
> When done, push and create PR: git push origin feature/sprint-5
> Follow Sections 12-14 of PROJECT_BRIEF.md.

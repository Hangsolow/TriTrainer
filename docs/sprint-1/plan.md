# Sprint 1 - Foundation Reboot

> Sprint Goal: Transform PoC into a product-ready foundation with new domain direction and a modernized UI baseline.
> Branch: feature/sprint-1
> Estimated effort: 1.5-2 weeks

## Prioritized Task List

| # | Task | Owner | Est | Description |
|---|------|-------|-----|-------------|
| 1 | Domain model blueprint | Sage | 4h | Define entities for athletes, goals, plans, sessions, and personal records |
| 2 | API contract draft | Sage | 4h | Specify endpoints and request/response contracts for plan/goals/records |
| 3 | UI information architecture | Nova + Kira | 4h | Define key screens and navigation replacing calendar-only PoC flow |
| 4 | Visual language baseline | Milo | 4h | Establish design tokens, spacing, typography, and component styling direction |
| 5 | Vertical slice implementation | Nova + Sage | 12h | Implement one end-to-end flow from plan setup to progress view |
| 6 | Migration safety tests | Ivy + Sage | 6h | Verify existing activity data remains usable after schema/domain expansion |
| 7 | Regression and sign-off prep | Ivy | 4h | Execute QA checklist, file issues, prepare sprint sign-off |

## Work Schedule

### Phase 1: Product and Architecture Baseline (tasks 1-3)
- Finalize scope and API/UI contracts.
- Checkpoint commit after phase.

### Phase 2: Design and Vertical Slice (tasks 4-5)
- Build core user flow with new UI baseline.
- Checkpoint commit after phase.

### Phase 3: QA Hardening and Handoff (tasks 6-7)
- Run migration/regression testing and produce handoff docs.
- Final commit and PR.

## Success Criteria

- [ ] New domain model and API contracts documented and implemented for at least one full flow.
- [ ] UI no longer limited to calendar-only workflow and reflects new product direction.
- [ ] Existing PoC activity data remains stable or has explicit migration strategy.
- [ ] QA sign-off file exists with blockers clearly reported.
- [ ] All relevant tests pass.
- [ ] No high-severity console/runtime errors in the shipped slice.

## What is NOT in This Sprint

| Feature | Reason |
|---------|--------|
| Full adaptive recommendation engine | Deferred to Sprint 2 to avoid rushed logic |
| Advanced analytics dashboards | Depends on richer data volume and completed event model |
| Social features and sharing | Out of scope for foundation reboot |

## Agent Prompt

> Read PROJECT_BRIEF.md, then read docs/sprint-1/plan.md. Execute Sprint 1.
>
> First: git pull origin main && git checkout -b feature/sprint-1
>
> Close GitHub Issues in commits: "fix: description (Fixes #NN)"
> Update docs/sprint-1/progress.md after each phase.
> When done, push and create PR: git push origin feature/sprint-1
> Follow Sections 12-14 of PROJECT_BRIEF.md.

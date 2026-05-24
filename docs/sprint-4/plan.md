# Sprint 4 - Release Gate Hardening

> Sprint Goal: Close Sprint 3 conditional merge gates with strict CI evidence, then harden distributed startup confidence and QA evidence flow without expanding major feature surface.
> Branch: feature/sprint-4
> Estimated effort: 1-1.5 weeks

## Prioritized Task List

| # | Task | Owner | Est | Description |
|---|------|-------|-----|-------------|
| 1 | CI branch evidence capture and ledger | Dash | 3h | Capture strict green UNIT lane run evidence on feature branch; publish run URL, artifact inventory, and gate mapping in sprint tracker |
| 2 | Playwright local evidence closure workflow | Ivy + Dash + Remy | 4h | Replace GitHub strict-smoke dependency with local trusted-machine evidence package and closure criteria |
| 3 | Aspire startup-health regression expansion | Ivy + Sage | 6h | Add/expand integration coverage for cross-service startup readiness and health endpoint behavior under AppHost orchestration |
| 4 | CI artifact quality and failure triage hardening | Dash + Ivy | 4h | Ensure test artifacts are complete, named consistently, and actionable for fast defect triage across unit/integration/playwright lanes |
| 5 | Sprint 4 QA and merge readiness package | Ivy + Remy | 3h | Produce QA sign-off, gate-by-gate merge recommendation, and complete handoff docs |

## Work Schedule

### Phase 1: Carry-over gate closure (Tasks 1-2)
- Close unresolved Sprint 3 conditional gates first.
- Publish objective evidence in progress tracker before any additional scope.
- Checkpoint commit after phase.

### Phase 2: Hardening execution (Tasks 3-4)
- Increase confidence in distributed startup and test evidence quality.
- Keep changes focused on reliability and verification outcomes.
- Checkpoint commit after phase.

### Phase 3: QA and handoff (Task 5)
- Confirm merge gates with explicit pass/fail evidence.
- Publish sign-off artifacts and sprint close package.
- Final commit and PR.

## Success Criteria

- [ ] First strict green feature-branch UNIT CI run evidence is attached in sprint tracker with artifact summary.
- [ ] Playwright gate is closed via local trusted-machine evidence, or documented waiver with owner, expiry date, and closure plan.
- [ ] Integration coverage includes cross-service startup/health regression checks for Aspire-orchestrated stack.
- [ ] GitHub CI artifacts are consistently published for unit lane and sufficient for one-pass triage.
- [ ] QA sign-off is PASS or CONDITIONAL PASS with approved exceptions and no open blocker defects.

## What is NOT in This Sprint

| Feature | Reason |
|---------|--------|
| Adaptive recommendation engine | Requires additional product and model design; not release-gate hardening |
| Auth / authorization rollout | Requires dedicated security sprint and threat model |
| Coach mode / multi-athlete management | Product expansion outside current release gate objective |
| Race simulation and taper planner | Backlog item with significant new domain logic |
| Blazor Interactive Auto migration | Platform-level migration not required for gate closure |
| Large UI redesign | Sprint objective is reliability and verification hardening |

## Agent Prompt

> Read PROJECT_BRIEF.md, then read docs/sprint-4/plan.md. Execute Sprint 4.
>
> First: git pull origin main && git checkout -b feature/sprint-4
>
> Close GitHub Issues in commits: "fix: description (Fixes #NN)"
> Update docs/sprint-4/progress.md after each phase.
> When done, push and create PR: git push origin feature/sprint-4
> Follow Sections 12-14 of PROJECT_BRIEF.md.

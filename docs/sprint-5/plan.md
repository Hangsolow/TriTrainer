# Sprint 5 - CI Trust Remediation and Gate Operations

> Sprint Goal: Build a sustainable trust and evidence model so integration and Playwright quality gates remain reliable, auditable, and ready for future hosted CI re-enablement.
> Branch: feature/sprint-5
> Estimated effort: 1 week

## Prioritized Task List

| # | Task | Owner | Est | Description |
|---|------|-------|-----|-------------|
| 1 | Cert-trust remediation design | Dash + Sage | 4h | Define options to re-enable hosted integration/Playwright safely (custom CA trust, self-hosted runner, tunnel/domain strategy), with recommendation and decision matrix |
| 2 | Local gate evidence standardization | Ivy + Dash | 4h | Standardize local integration/Playwright execution evidence format (commands, TRX naming, environment metadata, retention path) |
| 3 | Gate policy hardening docs | Remy + Ivy | 3h | Align merge gate language across sprint artifacts and establish explicit pass/fail rules for unit-hosted vs local lanes |
| 4 | Operational automation for local lanes | Dash + Nova | 5h | Add minimal scripts/checklists to run integration + Playwright locally and produce repeatable evidence artifacts |
| 5 | QA governance package and handoff | Ivy + Remy | 3h | Publish Sprint 5 QA sign-off package and merge recommendation with risk register updates |

## Work Schedule

### Phase 1: Trust and Policy Baseline (Tasks 1-2)
- Lock trust-remediation recommendation and evidence schema first.
- Keep execution constrained to reliability/governance outcomes.
- Checkpoint commit after phase.

### Phase 2: Operational Hardening (Tasks 3-4)
- Make lane rules and local evidence capture repeatable.
- Avoid feature-surface expansion.
- Checkpoint commit after phase.

### Phase 3: QA and Producer Closeout (Task 5)
- Validate gates with explicit pass/fail state.
- Publish sign-off and closeout package.

## Success Criteria

- [ ] Cert-trust remediation options are documented with one approved implementation path.
- [ ] Local integration and Playwright evidence format is standardized and used consistently.
- [ ] Merge gate policy clearly distinguishes hosted CI unit lane from local-only lanes.
- [ ] Local lane execution is operationalized with repeatable commands/checklists.
- [ ] QA sign-off artifact exists with PASS or CONDITIONAL PASS and explicit blocker status.

## What is NOT in This Sprint

| Feature | Reason |
|---------|--------|
| Adaptive recommendation engine | Product feature scope outside CI/gate operations objective |
| Auth / authorization implementation | Separate security sprint required |
| Coach mode / multi-athlete workflows | Product expansion outside current sprint objective |
| Race simulation / taper planning | Backlog item unrelated to gate operations |
| Major UI redesign | Not required for CI trust and governance hardening |

## Agent Prompt

> Read PROJECT_BRIEF.md, then read docs/sprint-5/plan.md. Execute Sprint 5.
>
> First: git pull origin main && git checkout -b feature/sprint-5
>
> Close GitHub Issues in commits: "fix: description (Fixes #NN)"
> Update docs/sprint-5/progress.md after each phase.
> When done, push and create PR: git push origin feature/sprint-5
> Follow Sections 12-14 of PROJECT_BRIEF.md.

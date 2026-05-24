# Sprint 5 - Producer Kickoff

Date: 2026-05-24
Owner: Remy (Producer)
Sprint: Athlete Experience Expansion

## Sprint 5 Producer Brief

Sprint 5 returns to product delivery. Sprint 4 closed release gates and stabilized governance; Sprint 5 focuses on athlete-visible value by shipping recommendation insights and smoother planning/records workflows.

Sprint objective:
- Ship recommendation insights API and dashboard UX (Tasks 1-2).
- Reduce friction in goal-to-plan creation workflows (Task 3).
- Improve records page daily usability (Task 4).
- Close sprint with QA validation and producer handoff package (Task 5).

Priority order:
1. Backend recommendation contracts first (Task 1).
2. Dashboard and workflow UX next (Tasks 2-4).
3. QA and producer closeout last (Task 5).

Out of scope:
- Hosted CI cert-trust remediation implementation.
- Full auth / authorization rollout.
- Coach mode / multi-athlete workflows.
- Race simulation / taper planning.
- Large cross-app redesign.

## Day 1 Checklist

1. Confirm task owners and backups:
   - Task 1: Sage
   - Task 2: Nova + Milo
   - Task 3: Nova + Sage
   - Task 4: Nova + Milo
   - Task 5: Ivy + Remy
2. Freeze Sprint 5 scope to Tasks 1-5 only; move expansions to docs/ideas-backlog.md.
3. Run 30-minute alignment:
   - Sage: recommendation contract shape and decision logic inputs.
   - Nova + Milo: dashboard and records UX acceptance criteria.
   - Ivy: QA acceptance matrix and regression focus for sprint features.
   - Remy: merge gates and closeout artifact expectations.
4. Record day-1 decisions in docs/sprint-5/progress.md.
5. Require first producer checkpoint by end of day 1.

## Parallel Chat Prompts

### @ai-team-devops

```text
@ai-team-devops
Sprint 5 DevOps kickoff: cert-trust remediation design and local lane operations hardening.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 14)
2) docs/sprint-5/plan.md
3) docs/sprint-5/progress.md

Tasks owned:
- Support feature delivery quality gates and keep CI policy stable while product tasks ship.

Requirements:
1) Keep GitHub CI unit lane healthy and non-disruptive to product sprint execution.
2) Support dev/qa with reproducible test execution guidance when regressions appear.
3) Update docs/sprint-5/progress.md only for risks/blockers impacting feature delivery.

Definition of done:
- CI policy remains stable and feature lanes are unblocked operationally.
```

### @ai-team-dev

```text
@ai-team-dev
Sprint 5 feature kickoff: build recommendation and workflow UX improvements.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 13)
2) docs/sprint-5/plan.md
3) docs/sprint-5/progress.md

Tasks owned:
- Task 1: recommendation insights API MVP.
- Task 2: dashboard recommendation widgets.
- Task 3: goal/plan quick-start improvements.
- Task 4: records usability improvements.

Requirements:
1) Keep scope to sprint-defined feature improvements; avoid infra policy work.
2) Ship backend contracts before frontend wiring where dependencies exist.
3) Record acceptance evidence and edge-case handling in sprint progress.

Definition of done:
- Tasks 1-4 feature outcomes are implemented and regression-safe.
```

### @ai-team-qa

```text
@ai-team-qa
Sprint 5 QA kickoff: feature validation and sprint sign-off.

Read in order:
1) PROJECT_BRIEF.md (Sections 8, 12, 13)
2) docs/sprint-5/plan.md
3) docs/sprint-5/progress.md

Tasks owned:
- Task 5: QA package and sign-off.
- Support verification for Tasks 1-4 feature behavior and regression safety.

Requirements:
1) Validate recommendation, dashboard, goal/plan, and records feature acceptance criteria.
2) Confirm no blocker regressions in core planning/progress flows.
3) Publish sign-off in docs/qa/sprint-5-signoff.md with clear blocker status.

Exit criteria:
- Feature acceptance package is complete and reproducible.
- PASS or CONDITIONAL PASS issued with explicit rationale.
```

### @ai-team-producer checkpoint lane

```text
@ai-team-producer
Run Sprint 5 producer checkpoint lane daily until merge.

Inputs:
- docs/sprint-5/plan.md
- docs/sprint-5/progress.md
- PROJECT_BRIEF.md sections 7, 8, 13
- Open GitHub Issues for sprint scope

Checkpoint workflow:
1) Validate progress against Tasks 1-5 and call out slippage by task number.
2) Detect scope creep; move non-Sprint-5 asks to backlog immediately.
3) Confirm each blocker has owner, severity, ETA, and escalation path.
4) Enforce defect hygiene and sign-off clarity.
5) Enforce merge policy: regular merge only after gates pass.

Checkpoint output format:
- Status: on-track/off-track
- Task delta: completed/in-progress/blocked by task number
- New risks: top items with mitigation owner
- Merge readiness: pass/fail per gate
- Next 24h focus
```

## Risks and Mitigations

1. Recommendation logic may be too generic to provide athlete value.
   - Mitigation: Sage defines concrete recommendation rules tied to measurable plan/progress signals.

2. UI scope may expand and delay sprint closure.
   - Mitigation: Nova + Milo lock acceptance criteria for dashboard and records before implementation.

3. Feature changes may regress existing planning/progress flows.
   - Mitigation: Ivy validates focused regression paths and escalates blockers immediately.

## Merge Gates

1. Task completion gate: Tasks 1-5 complete or explicitly deferred with producer approval.
2. Feature acceptance gate: recommendation/dashboard/goal-plan/records improvements meet documented acceptance criteria.
3. Regression gate: core planning, progress, and records flows remain functional.
4. CI gate: GitHub unit-only CI lane remains green.
5. Defect gate: zero open severity:blocker issues; severity:major requires explicit producer + QA acceptance.
6. QA sign-off gate: docs/qa/sprint-5-signoff.md exists with PASS or CONDITIONAL PASS.
7. Documentation gate: PROJECT_BRIEF.md section 8 updated, docs/sprint-5/progress.md current, docs/sprint-5/done.md present.
8. Merge policy gate: regular merge only; no squash, no rebase.

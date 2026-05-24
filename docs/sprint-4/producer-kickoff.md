# Sprint 4 - Producer Kickoff

Date: 2026-05-22
Owner: Remy (Producer)
Sprint: Release Gate Hardening

## Sprint 4 Producer Brief

Sprint 4 is a confidence sprint. Sprint 3 shipped substantial product value but closed with conditional gates. Sprint 4 resolves that condition first, then tightens reliability around distributed startup and evidence quality so merge decisions are unambiguous.

Sprint objective:
- Close carry-over CI evidence gate by capturing and recording first strict green UNIT branch run (Task 1).
- Close Playwright gate by publishing local trusted-machine proof or an approved waiver package with expiry (Task 2).
- Expand Aspire startup-health regression confidence across cross-service readiness behavior (Task 3).
- Harden artifact quality and failure triage evidence to reduce diagnosis time (Task 4).
- Complete QA sign-off and producer merge package with explicit gate outcomes (Task 5).

Priority order:
1. Carry-over gate closure first (Tasks 1-2).
2. Reliability hardening next (Tasks 3-4).
3. QA and producer handoff last (Task 5).

Out of scope:
- Adaptive recommendation engine.
- Auth / authorization implementation.
- Coach mode / multi-athlete workflows.
- Race simulation / taper planning.
- Interactive Auto migration.
- Broad visual redesign.

## Day 1 Checklist

1. Confirm task owners and backup owners:
   - Task 1: Dash
   - Task 2: Ivy + Dash + Remy
   - Task 3: Ivy + Sage
   - Task 4: Dash + Ivy
   - Task 5: Ivy + Remy
2. Freeze Sprint 4 scope to Tasks 1-5 only; route all expansion ideas to docs/ideas-backlog.md.
3. Run 30-minute gate-closure alignment:
   - Dash: CI run evidence structure and artifact inventory template.
   - Ivy: Playwright strict closure criteria and waiver metadata requirements.
   - Sage: startup-health regression target matrix under AppHost orchestration.
4. Publish day-1 decisions in docs/sprint-4/progress.md:
   - Evidence schema for Task 1.
   - Waiver lifecycle rules for Task 2.
   - Startup-health matrix for Task 3.
5. Require first checkpoint update by end of day 1 with status, blockers, and gate impacts.

## Parallel Chat Prompts

### @ai-team-devops

```text
@ai-team-devops
Sprint 4 DevOps kickoff: close carry-over CI evidence gates and harden artifacts.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 14)
2) docs/sprint-4/plan.md
3) docs/sprint-4/progress.md

Tasks owned:
- Task 1: CI branch evidence capture and ledger.
- Task 4 (with Ivy): artifact quality and failure triage hardening.

Requirements:
1) Capture first strict green UNIT run evidence on feature/sprint-4 with run URL, commit SHA, job result, and artifact list.
2) Record evidence package in docs/sprint-4/progress.md under Task 1 notes.
3) Ensure unit artifact naming is consistent and traceable by lane.
4) Keep test execution aligned with TUnit executable convention (`dotnet run --project ... --configuration Release`).
5) Escalate any CI instability immediately with owner + ETA in progress tracker.

Definition of done:
- Task 1 evidence package published and producer-reviewable.
- Task 4 artifact triage quality accepted by QA owner for GitHub unit lane plus local lane evidence handoff.
```

### @ai-team-dev

```text
@ai-team-dev
Sprint 4 backend/support kickoff: startup-health reliability hardening for distributed app behavior.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 13)
2) docs/sprint-4/plan.md
3) docs/sprint-4/progress.md

Task owned:
- Task 3 (with Ivy): Aspire startup-health regression expansion.

Requirements:
1) Expand integration coverage for cross-service startup readiness and health endpoint behavior.
2) Focus on practical reliability checks, not broad feature expansion.
3) Keep API contract behavior consistent with existing validation semantics.
4) Publish test matrix and handoff notes in docs/sprint-4/progress.md.

Definition of done:
- Startup-health regression cases are implemented and passing in agreed execution lane.
- No open blocker defects in this task lane.
```

### @ai-team-qa

```text
@ai-team-qa
Sprint 4 QA kickoff: strict gate closure and hardening verification.

Read in order:
1) PROJECT_BRIEF.md (Sections 8, 12, 13)
2) docs/sprint-4/plan.md
3) docs/sprint-4/progress.md

Tasks owned:
- Task 2 (with Dash + Remy): Playwright strict evidence closure and waiver workflow.
- Task 3 (with Sage): startup-health regression verification.
- Task 5 (with Remy): QA sign-off and merge recommendation.

Requirements:
1) Verify Playwright local strict pass evidence on trusted dev machine; if blocked, produce waiver package with owner, expiry, and closure criteria.
2) Validate startup-health regression coverage and report any gaps.
3) File GitHub Issues for defects with labels: bug + severity:blocker|major|minor.
4) Publish sign-off recommendation in docs/qa/sprint-4-signoff.md.

Exit criteria:
- No open blocker defects.
- Gate evidence package complete for producer merge review.
- PASS or CONDITIONAL PASS with approved exceptions.
```

### @ai-team-producer checkpoint lane

```text
@ai-team-producer
Run Sprint 4 producer checkpoint lane daily until merge.

Inputs:
- docs/sprint-4/plan.md
- docs/sprint-4/progress.md
- PROJECT_BRIEF.md sections 7, 8, 13
- Open GitHub Issues for sprint scope

Checkpoint workflow:
1) Validate progress against Tasks 1-5 and call out slippage by task number.
2) Detect scope creep; move non-Sprint-4 asks to backlog immediately.
3) Confirm each blocker has owner, severity, ETA, and escalation path.
4) Enforce defect hygiene: severity labels and reproducible steps.
5) Enforce merge policy: regular merge only after gates pass.

Checkpoint output format:
- Status: on-track/off-track
- Task delta: completed/in-progress/blocked by task number
- New risks: top items with mitigation owner
- Merge readiness: pass/fail per gate
- Next 24h focus
```

## Risks and Mitigations

1. Carry-over CI evidence is delayed by branch instability in unit lane.
   - Mitigation: Dash publishes first-failure triage within same day and records retry evidence chain.

2. Playwright local strict gate remains unstable on developer machines.
   - Mitigation: Ivy + Dash apply waiver protocol with expiration date and mandatory closure plan tracked in progress.

3. Startup-health regressions surface intermittent readiness timing issues.
   - Mitigation: Sage + Ivy define deterministic readiness assertions and preserve evidence artifacts per run.

4. Scope creep reintroduces feature expansion and threatens gate closure timeline.
   - Mitigation: Remy enforces out-of-scope policy and moves all expansions to backlog on sight.

## Merge Gates

1. Task completion gate: Tasks 1-5 complete or explicitly deferred with producer approval.
2. CI evidence gate: strict green UNIT branch run evidence is attached and traceable.
3. Playwright gate: local strict pass evidence attached, or waiver approved with owner + expiry + closure criteria.
4. Regression gate: startup-health regression cases pass in local trusted environment and baseline suites remain green.
5. Defect gate: zero open severity:blocker issues; severity:major requires explicit producer + QA acceptance.
6. QA sign-off gate: docs/qa/sprint-4-signoff.md exists with PASS or CONDITIONAL PASS.
7. Documentation gate: PROJECT_BRIEF.md section 8 updated, docs/sprint-4/progress.md current, docs/sprint-4/done.md present.
8. Merge policy gate: regular merge only; no squash, no rebase.

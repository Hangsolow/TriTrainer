# Sprint 5 - Producer Kickoff

Date: 2026-05-24
Owner: Remy (Producer)
Sprint: CI Trust Remediation and Gate Operations

## Sprint 5 Producer Brief

Sprint 5 protects delivery confidence by resolving the root operational gap left after Sprint 4: hosted CI cannot currently validate integration and Playwright lanes due certificate trust constraints. This sprint defines the remediation path and operationalizes local lane evidence so merge governance remains objective and repeatable.

Sprint objective:
- Define and approve cert-trust remediation strategy for hosted CI lane re-enablement (Task 1).
- Standardize local lane evidence capture and naming for integration/Playwright (Task 2).
- Harden merge-gate policy documentation and acceptance criteria (Task 3).
- Operationalize local lane execution into repeatable scripts/checklists (Task 4).
- Close sprint with QA governance package and producer merge recommendation (Task 5).

Priority order:
1. Strategy and policy first (Tasks 1-3).
2. Operational repeatability second (Task 4).
3. QA and producer closeout last (Task 5).

Out of scope:
- Adaptive recommendation engine.
- Auth / authorization implementation.
- Coach mode / multi-athlete workflows.
- Race simulation / taper planning.
- Major UI redesign.

## Day 1 Checklist

1. Confirm task owners and backups:
   - Task 1: Dash + Sage
   - Task 2: Ivy + Dash
   - Task 3: Remy + Ivy
   - Task 4: Dash + Nova
   - Task 5: Ivy + Remy
2. Freeze Sprint 5 scope to Tasks 1-5 only; move expansions to docs/ideas-backlog.md.
3. Run 30-minute alignment:
   - Dash: trust-remediation options and infrastructure risk.
   - Sage: technical feasibility/security implications for cert strategy.
   - Ivy: QA evidence acceptance and rerun reliability thresholds.
   - Remy: merge gate definitions and sign-off format.
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
- Task 1: Cert-trust remediation design.
- Task 4 (with Nova): operational automation for local lanes.

Requirements:
1) Produce decision matrix for hosted CI trust remediation options and select recommended path.
2) Keep current policy intact during sprint (GitHub CI unit-only) unless producer explicitly approves policy change.
3) Provide repeatable commands/checklists or scripts for local integration/Playwright runs with evidence output.
4) Update docs/sprint-5/progress.md after each task with risks, owner, ETA, and gate impact.

Definition of done:
- Strategy recommendation documented and producer-reviewable.
- Local lane operation artifacts are repeatable and traceable.
```

### @ai-team-dev

```text
@ai-team-dev
Sprint 5 support kickoff: implement minimal operational improvements that stabilize local gate execution.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 13)
2) docs/sprint-5/plan.md
3) docs/sprint-5/progress.md

Task owned:
- Task 4 (with Dash): operational automation for local lanes.

Requirements:
1) Keep scope minimal and operational; no broad feature changes.
2) Ensure any helper scripts/checklists match existing test conventions.
3) Record what changed and why in sprint progress.

Definition of done:
- Local lane execution is easier to run and produces predictable evidence artifacts.
```

### @ai-team-qa

```text
@ai-team-qa
Sprint 5 QA kickoff: governance and evidence standardization validation.

Read in order:
1) PROJECT_BRIEF.md (Sections 8, 12, 13)
2) docs/sprint-5/plan.md
3) docs/sprint-5/progress.md

Tasks owned:
- Task 2: Local gate evidence standardization.
- Task 5: QA governance package and sign-off.

Requirements:
1) Validate evidence schema for local integration and Playwright lanes.
2) Confirm reproducibility of evidence collection steps.
3) Publish sign-off in docs/qa/sprint-5-signoff.md with clear blocker status.

Exit criteria:
- Evidence package is complete and audit-ready.
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

1. Hosted cert-trust remediation option is selected but not operationally viable.
   - Mitigation: Dash + Sage include validation criteria and rollback plan in the decision matrix.

2. Local evidence format diverges across machines and weakens audit quality.
   - Mitigation: Ivy defines mandatory evidence schema and acceptance checklist.

3. Scope creep reintroduces product work into an operations sprint.
   - Mitigation: Remy enforces out-of-scope policy and redirects to docs/ideas-backlog.md.

## Merge Gates

1. Task completion gate: Tasks 1-5 complete or explicitly deferred with producer approval.
2. CI evidence gate: GitHub unit-only CI evidence remains green and traceable.
3. Local execution gate: integration and Playwright local evidence is captured in standardized format.
4. Policy gate: merge-gate policy language is consistent across sprint docs and QA sign-off.
5. Defect gate: zero open severity:blocker issues; severity:major requires explicit producer + QA acceptance.
6. QA sign-off gate: docs/qa/sprint-5-signoff.md exists with PASS or CONDITIONAL PASS.
7. Documentation gate: PROJECT_BRIEF.md section 8 updated, docs/sprint-5/progress.md current, docs/sprint-5/done.md present.
8. Merge policy gate: regular merge only; no squash, no rebase.

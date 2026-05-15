# Sprint 2 - Producer Kickoff

Date: 2026-05-15
Owner: Remy (Producer)
Sprint: Plan and Goals Core

## Sprint 2 Producer Brief

Sprint 2 hardens the Sprint 1 vertical slice. This sprint is not about expanding feature surface; it is about making plans and goals behavior reliable, validated, and testable end-to-end under the existing C# + Aspire architecture.

Sprint objective:
- Improve plan auto-generation quality with discipline templates and weekly defaults (Task 1).
- Enforce strict goal and plan validation semantics on `/v1` endpoints (Task 2).
- Refine Plans UX so generated sessions are understandable and actionable (Task 3).
- Refine Progress UX so variance and compliance are easier to interpret (Task 4).
- Expand API regression coverage for success and failure behavior in hardening areas (Task 5).
- Add Playwright smoke path for Dashboard -> Goals -> Plans -> Progress -> Records (Task 6).
- Close sprint with QA sign-off and complete handoff artifacts (Task 7).

Priority order:
1. Backend rules and validation (Tasks 1-2).
2. UX refinement over existing pages (Tasks 3-4).
3. Test and automation hardening (Tasks 5-6).
4. Producer handoff and merge control (Task 7).

Out of scope:
- Adaptive recommendation engine logic.
- Auth or security model implementation.
- Broad new API surface not required for plan and goals hardening.

## Day 1 Checklist

1. Confirm owners and pairing plan for Tasks 1-7, including who owns cross-lane reviews.
2. Freeze scope to Sprint 2 hardening tasks only; move all expansion ideas to backlog.
3. Hold a 45-minute contract and behavior alignment:
   - Sage: define exact generation rule behavior and validation error semantics.
   - Nova and Kira: define Plans and Progress UX acceptance checkpoints.
   - Ivy: define regression matrix and E2E smoke acceptance criteria.
4. Publish day-1 decision log in sprint progress:
   - Rule set v1 decisions for generation.
   - Validation semantics and known edge cases.
   - UI behavior expectations for generated sessions and compliance display.
5. Start parallel execution:
   - Backend starts Tasks 1-2 with test-first checks for failure paths.
   - Frontend starts Tasks 3-4 after behavior contract freeze.
   - QA starts Task 5 planning and Task 6 flow mapping before implementation lands.
6. Require first checkpoint update by end of day covering status, blockers, and any scope drift.
7. Producer verifies no blocker risks are ignored and assigns owner plus ETA to each active risk.

## Parallel Chat Prompts

### @ai-team-dev

Copy-paste prompt:

```text
@ai-team-dev
Sprint 2 kickoff: Plan and Goals Core. Execute Tasks 1-4 first, then support Task 5 test hardening.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 13, 14)
2) docs/sprint-2/plan.md
3) docs/sprint-2/progress.md

Scope and constraints:
- Goal: harden Sprint 1 vertical slice.
- Hard constraints: C# backend, Aspire local orchestration.
- In scope: Tasks 1-7 only.
- Out of scope: recommendation engine, auth rollout, broad new endpoints.

Execution requirements:
1) Implement Task 1 plan generation rule set v1 with discipline templates + weekly defaults.
2) Implement Task 2 validation hardening with explicit invalid transition and payload semantics.
3) Implement Task 3 and Task 4 UX refinements only for generated session clarity and progress readability.
4) Support Task 5 by ensuring tests can assert deterministic generation + validation behavior.
5) Preserve Sprint 1 behavior and avoid breaking legacy activities flow.
6) Update docs/sprint-2/progress.md after each phase with blockers, decisions, and changed files.

Definition of done for dev lane:
- Tasks 1-4 complete and demonstrable.
- No unresolved blocker defects owned by dev lane.
- Handoff notes prepared for QA and producer merge review.
```

### @ai-team-qa

Copy-paste prompt:

```text
@ai-team-qa
Sprint 2 QA kickoff: validate hardening quality for plans/goals and smoke E2E readiness.

Read in order:
1) PROJECT_BRIEF.md (Sections 8, 12, 13)
2) docs/sprint-2/plan.md
3) docs/sprint-2/progress.md

Mission:
- Own Task 5 API regression coverage expansion.
- Own Task 6 Playwright smoke suite v1.
- Provide sign-off input for Task 7.

QA requirements:
1) Build and execute a regression matrix for:
   - Generation rules (discipline templates and weekly defaults).
   - Goal/plan status transition constraints.
   - Invalid payload behavior and error semantics on `/v1` endpoints.
2) Implement and run smoke flow: Dashboard -> Goals -> Plans -> Progress -> Records.
3) Verify Sprint 1 legacy behavior remains intact (especially activities support flow).
4) File GitHub Issues for defects with labels: bug + severity:blocker|major|minor.
5) Publish QA status and residual risk in docs/qa/sprint-2-signoff.md.

Exit criteria for QA lane:
- No blocker defects open.
- Smoke suite passes for core path.
- Sign-off recommendation is PASS or CONDITIONAL PASS with documented exceptions.
```

### @ai-team-producer checkpoint lane

Copy-paste prompt:

```text
@ai-team-producer
Run Sprint 2 producer checkpoint lane daily until merge.

Inputs:
- docs/sprint-2/plan.md
- docs/sprint-2/progress.md
- PROJECT_BRIEF.md sections 7, 8, 13
- Open GitHub Issues for sprint scope

Checkpoint workflow:
1) Validate progress against Tasks 1-7 and call out slippage by task number.
2) Detect scope creep; move non-hardening asks to backlog immediately.
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

1. Non-deterministic generation outputs make API and E2E tests flaky.
- Mitigation: lock deterministic defaults for Task 1 and require test fixtures that assert stable generated sessions.

2. Validation hardening changes error semantics and breaks Sprint 1 client assumptions.
- Mitigation: define and freeze expected error contract early in Task 2, then verify through Task 5 regression cases before merge.

3. Plans UX updates display generated sessions differently than API payload meaning.
- Mitigation: add explicit acceptance examples linking API fields to UI rendering during Tasks 3-4 review.

4. E2E smoke path fails under Aspire startup timing and service discovery warm-up.
- Mitigation: in Task 6, add readiness checks and stable test sequencing tied to health endpoints before navigation assertions.

5. Hardening work regresses legacy activities flow while touching shared API or UI paths.
- Mitigation: require legacy activity coexistence checks in Task 5 and block merge if any regression appears.

## Merge Gates

1. Task completion gate: Tasks 1-7 are complete or explicitly deferred with producer approval documented in docs.
2. Validation gate: Task 2 rules reject invalid plan/goal transitions and malformed payloads with consistent error semantics.
3. Regression gate: Task 5 covers generation rules, validation failures, and legacy activities coexistence with passing results.
4. Smoke gate: Task 6 Playwright path Dashboard -> Goals -> Plans -> Progress -> Records passes in local Aspire run.
5. Defect gate: zero open `severity:blocker` issues; any `severity:major` requires explicit producer + QA acceptance note.
6. QA sign-off gate: docs/qa/sprint-2-signoff.md exists and indicates PASS or CONDITIONAL PASS with approved exceptions.
7. Documentation gate: PROJECT_BRIEF.md section 8 updated, docs/sprint-2/progress.md current, docs/sprint-2/done.md present before merge.
8. Merge policy gate: PR merged with regular merge only; no squash and no rebase.

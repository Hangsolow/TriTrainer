# Sprint 6 - Producer Kickoff

Date: 2026-05-24
Owner: Remy (Producer)
Sprint: Guidance Precision and Daily Flow

## Sprint 6 Producer Brief

Sprint 6 builds on Sprint 5 by improving recommendation precision in context and reducing friction in daily athlete actions.

Sprint objective:
- Deep-link recommendation CTA flows to relevant context (Task 1).
- Add explainable readiness visibility and quick logging improvements (Tasks 2 and 3).
- Calibrate recommendation behavior and edge-case handling (Task 4).
- Close sprint with QA validation and producer handoff package (Task 5).

Priority order:
1. Contextual recommendation flow and contract calibration first (Tasks 1 and 4).
2. Daily UX flow improvements next (Tasks 2 and 3).
3. QA and producer closeout last (Task 5).

Out of scope:
- Hosted CI cert-trust remediation implementation.
- Full auth/authorization rollout.
- Coach mode / multi-athlete workflows.
- Race simulation / taper planning.
- Large cross-app redesign.

## Day 1 Checklist

1. Confirm task owners and backups:
   - Task 1: Nova + Sage
   - Task 2: Nova + Milo + Sage
   - Task 3: Nova + Milo
   - Task 4: Sage + Ivy
   - Task 5: Ivy + Remy
2. Freeze Sprint 6 scope to Tasks 1-5 only; move expansions to docs/ideas-backlog.md.
3. Run 30-minute alignment:
   - Sage: deep-link and contract constraints.
   - Nova + Milo: readiness/quick-log UX acceptance criteria.
   - Ivy: QA matrix for recommendation and daily flow regressions.
   - Remy: merge gates and closeout expectations.
4. Record day-1 decisions in docs/sprint-6/progress.md.
5. Require first producer checkpoint by end of day 1.

## Merge Gates

1. Task completion gate: Tasks 1-5 complete or explicitly deferred with producer approval.
2. Feature acceptance gate: recommendation deep-links, readiness card, and quick-log improvements meet documented criteria.
3. Regression gate: core planning/progress/records flows remain functional.
4. CI gate: GitHub unit-only CI lane remains green.
5. Defect gate: zero open severity:blocker issues; severity:major requires explicit producer + QA acceptance.
6. QA sign-off gate: docs/qa/sprint-6-signoff.md exists with PASS or CONDITIONAL PASS.
7. Documentation gate: PROJECT_BRIEF.md section 8 updated, docs/sprint-6/progress.md current, docs/sprint-6/done.md present.
8. Merge policy gate: regular merge only; no squash, no rebase.

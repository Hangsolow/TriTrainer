# Sprint 6 - Producer Kickoff

Date: 2026-05-24
Owner: Remy (Producer)
Sprint: UX Acceleration

## Sprint 6 Producer Brief

Sprint 6 focuses on athlete-facing UX improvements by reducing friction and improving contextual guidance in core daily flows.

Sprint objective:
- Deep-link recommendation actions to meaningful context (Task 1).
- Improve dashboard readability and hierarchy (Task 2).
- Reduce records workflow friction and improve quick actions (Task 3).
- Add a faster daily check-in path from dashboard (Task 4).
- Close sprint with QA validation and producer handoff package (Task 5).

Priority order:
1. Contextual navigation first (Task 1).
2. Dashboard and records UX quality next (Tasks 2-3).
3. Quick check-in path then closeout (Tasks 4-5).

Out of scope:
- Hosted CI cert-trust remediation implementation.
- Full auth / authorization rollout.
- Coach mode / multi-athlete workflows.
- Race simulation / taper planning.
- Large cross-app redesign.

## Day 1 Checklist

1. Confirm task owners and backups:
   - Task 1: Nova + Sage
   - Task 2: Nova + Milo
   - Task 3: Nova + Milo
   - Task 4: Nova + Sage
   - Task 5: Ivy + Remy
2. Freeze Sprint 6 scope to Tasks 1-5 only; move expansions to docs/ideas-backlog.md.
3. Run 30-minute alignment:
   - Sage: contextual link and contract constraints.
   - Nova + Milo: dashboard/records UX acceptance criteria.
   - Ivy: QA matrix for UX improvements and regression safety.
   - Remy: merge gates and closeout artifact expectations.
4. Record day-1 decisions in docs/sprint-6/progress.md.
5. Require first producer checkpoint by end of day 1.

## Merge Gates

1. Task completion gate: Tasks 1-5 complete or explicitly deferred with producer approval.
2. Feature acceptance gate: UX improvements meet documented acceptance criteria.
3. Regression gate: core planning/progress/records flows remain functional.
4. CI gate: GitHub unit-only CI lane remains green.
5. Defect gate: zero open severity:blocker issues; severity:major requires explicit producer + QA acceptance.
6. QA sign-off gate: docs/qa/sprint-6-signoff.md exists with PASS or CONDITIONAL PASS.
7. Documentation gate: PROJECT_BRIEF.md section 8 updated, docs/sprint-6/progress.md current, docs/sprint-6/done.md present.
8. Merge policy gate: regular merge only; no squash, no rebase.

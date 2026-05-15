# Sprint 1 - Producer Kickoff

Date: 2026-05-15
Owner: Remy (Producer)

## Sprint 1 Producer Brief

Sprint 1 is a Foundation Reboot sprint focused on replacing the calendar-only PoC direction with a product-ready baseline for plans, goals, and personal records while keeping C# backend and Aspire local orchestration unchanged.

Sprint objective:
- Establish a clear domain and contract baseline for athletes, goals, plans, sessions, and personal records.
- Deliver one end-to-end vertical slice from plan setup to progress view.
- Create a modern UI baseline and information architecture that moves beyond the current PoC interaction model.
- Protect existing activity data behavior through migration safety checks.
- Exit sprint with QA sign-off and handoff artifacts complete.

Scope priority order:
1. Domain model blueprint and API contract draft.
2. UI information architecture and visual language baseline.
3. Vertical slice implementation.
4. Migration safety and regression prep.
5. QA sign-off artifact and issue triage closure.

Out of scope this sprint:
- Adaptive recommendation engine.
- Advanced analytics dashboards.
- Social or sharing features.

Definition of sprint success:
- At least one complete plans or goals flow works end-to-end.
- UI baseline clearly reflects new product direction.
- Existing activity data is preserved or explicitly migrated.
- QA sign-off is present with severity-labeled defects handled.
- No blocker defects open at merge time.

## Day 1 Checklist

1. Confirm sprint branch strategy and owners for Product, Backend, Frontend, Art, QA.
2. Re-read sprint goal and current-state gaps, then freeze day-1 scope to tasks 1 to 3 only.
3. Run a 45-minute alignment session:
   - Product: athlete journey and acceptance criteria for the first vertical slice.
   - Backend: entity boundaries and initial endpoint contracts.
   - Frontend and Art: screen map and visual baseline decisions.
4. Publish day-1 decision log:
   - Chosen vertical slice.
   - Contract-first decisions.
   - Deferred decisions and owners.
5. Start execution in parallel:
   - Backend drafts contracts and domain shape for the selected slice.
   - Frontend defines navigation and page skeleton for the same slice.
   - Art defines token-level visual baseline.
6. QA prepares validation matrix and migration safety scenarios before implementation lands.
7. Producer checks for scope creep at end of day.
8. Producer posts checkpoint status with blockers and next-day focus.

## Parallel Chat Prompts

### Prompt A - Dev Execution

You are the implementation team for Sprint 1 Foundation Reboot. Execute only in-scope work for this sprint.

Context:
- Constraints: C# backend, Aspire local orchestration.
- Sprint focus: plans, goals, personal records direction with one vertical slice.
- Current state: PoC exists and must evolve to a product-ready baseline.
- Priorities today: domain blueprint, API contract draft, UI information architecture, visual baseline alignment.

Execution requirements:
1. Select one vertical slice and lock it before coding.
2. Work contract-first: define request and response models plus validation before endpoint logic.
3. Ensure frontend flow and backend contracts match exactly for the chosen slice.
4. Keep migration safety in mind for existing activity data behavior.
5. Update sprint progress after each phase with decisions, blockers, and changed scope.
6. Keep out-of-scope features out of the sprint branch.

Deliverables:
- Domain model baseline for sprint entities.
- API contract baseline for the chosen slice.
- UI flow baseline replacing calendar-only direction.
- Working vertical slice implementation.
- Notes for QA on expected behavior and known limitations.

### Prompt B - QA Prep and Validation

You are QA for Sprint 1 Foundation Reboot. Prepare validation early and run risk-first testing as soon as the vertical slice is available.

Context:
- Sprint includes domain and UI expansion beyond PoC.
- Highest risk: regressions in existing activity behavior and mismatched frontend/backend contracts.
- Defect tracking requires severity labels: blocker, major, minor.

QA execution:
1. Build a validation matrix with three lanes:
   - Legacy behavior protection.
   - New vertical slice happy-path and edge-path checks.
   - Data and migration safety checks.
2. Define pass or fail criteria for endpoint contract conformance, UI to API integration, and activity data continuity.
3. Validate Aspire-local end-to-end workflow for the slice.
4. File defects with reproducible steps, expected vs actual, and severity.
5. Publish sprint sign-off status with blockers and residual risks.

Outputs required:
- QA checklist and executed results.
- Defect list with severity and owner.
- Sign-off recommendation: pass, conditional pass, or fail.

### Prompt C - Producer Checkpoint and Recovery

You are the Producer checkpoint lane for Sprint 1. Maintain continuity, protect scope, and keep merge readiness visible.

Checkpoint duties:
1. Verify sprint scope remains inside Foundation Reboot objectives.
2. Confirm day-1 outputs exist:
   - Vertical slice selection.
   - Domain and contract decisions.
   - UI architecture and visual baseline decisions.
3. Enforce updates to sprint progress artifacts after each phase.
4. Track blockers with owner and ETA; escalate blockers immediately.
5. Run recovery protocol if context breaks:
   - Reconstruct state from sprint progress and brief status sections.
   - Publish next actionable task only.
6. Before merge recommendation, verify QA sign-off status and open defect severity.

Output format each checkpoint:
- Status: on-track or off-track.
- Scope drift: none or listed items moved to backlog.
- Blockers: owner, impact, ETA.
- Next critical action for next 24 hours.

## Risks and Mitigations

1. Scope creep from adding analytics or recommendation logic too early.
- Mitigation: enforce strict in-scope list at daily checkpoint; move extras to backlog.

2. Frontend and backend drift on contracts during parallel execution.
- Mitigation: contract-first workflow and same-day contract freeze; QA validates schema conformance early.

3. Regression of existing activity behavior during domain expansion.
- Mitigation: migration safety scenarios required before merge; block merge on unresolved data continuity issues.

4. UI redesign consuming sprint capacity and delaying vertical slice completion.
- Mitigation: time-box visual baseline to tokens and core component decisions; prioritize functional slice over polish.

5. Incomplete handoff causing context loss across chats.
- Mitigation: mandatory phase-by-phase progress updates and sprint done artifact completion.

## Merge Gates

1. Vertical slice gate: one complete flow from setup to progress view is functional end-to-end.
2. Contract gate: implemented API behavior matches agreed sprint contracts for the selected slice.
3. Regression gate: existing activity behavior remains valid or has explicit, tested migration handling.
4. QA gate: sign-off produced and zero blocker defects open.
5. Scope gate: no out-of-scope features merged under Sprint 1 branch.
6. Documentation gate: progress and done artifacts are complete and accurate.
7. Merge policy gate: regular merge only, with no squash and no rebase.

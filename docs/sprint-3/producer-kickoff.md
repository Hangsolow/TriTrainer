# Sprint 3 - Producer Kickoff

Date: 2026-05-15
Owner: Remy (Producer)
Sprint: Progress and PR Tracking

## Sprint 3 Producer Brief

Sprint 3 makes the athlete's effort visible. Sprint 2 delivered reliable plan generation and validation. Sprint 3 closes the loop: athletes can see how their completed sessions compare to the plan, track personal records, and read compliance trends — all backed by a CI pipeline that proves everything is green before it ships.

Sprint objective:
- Stand up GitHub Actions CI so every commit is automatically built and tested (Task 1).
- Execute the Playwright smoke suite carried forward from Sprint 2 inside CI (Task 2).
- Harden and expand the Personal Records API with discipline filtering and personal-best queries (Task 3).
- Add progress summary and streak analytics to the backend (Task 4).
- Build the Records page with discipline tabs, PR highlighting, and trend sparklines (Task 5).
- Build compliance dashboard widgets: rolling 4-week chart and streak display (Task 6).
- Expand regression coverage to cover new analytics and records behavior (Task 7).
- Close sprint with QA sign-off and full handoff artifacts (Task 8).

Priority order:
1. CI pipeline and Playwright execution first (Tasks 1-2) — unblocks all future quality gates.
2. Backend contracts before frontend (Tasks 3-4).
3. Frontend features over verified contracts (Tasks 5-6).
4. Test hardening and sprint close (Tasks 7-8).

Out of scope:
- Adaptive recommendation engine.
- Auth / authorization rollout.
- Coach mode or multi-athlete support.
- Race simulation / taper planner.
- Blazor Interactive Auto render mode migration.

## Day 1 Checklist

1. Confirm owners for Tasks 1-8; Dash owns CI, Ivy co-owns Playwright execution, Sage owns backend, Nova+Milo+Kira own frontend.
2. Freeze scope to Sprint 3 tasks only; route any expansion ideas to `docs/ideas-backlog.md`.
3. Behavior alignment session (30 min):
   - Dash: define CI workflow structure, runner choice, artifact strategy.
   - Sage: define `/v1/records` GET-by-discipline and personal-best query contract; define `/v1/progress/summary` shape and streak algorithm.
   - Nova + Kira: define Records page acceptance criteria and dashboard widget specs.
   - Ivy: confirm Playwright pre-requisites and smoke assertion strategy for CI environment.
4. Publish day-1 decision log in docs/sprint-3/progress.md:
   - CI runner and trigger strategy.
   - Progress summary API contract (fields, time window, streak definition).
   - Records page acceptance examples.
5. Parallel execution after contract freeze:
   - Dash: Task 1 CI skeleton. Ivy: unblock Task 2 once CI is green.
   - Sage: Tasks 3-4.
   - Nova + Milo: Tasks 5-6 after Task 3-4 contracts are stable.
   - Ivy: Task 7 planning runs in parallel; execution after Tasks 3-4 land.
6. Require first checkpoint update by end of day 1 covering status, blockers, and any scope drift.

## Parallel Chat Prompts

### @ai-team-devops

```text
@ai-team-devops
Sprint 3 DevOps kickoff: stand up CI and enable Playwright smoke execution.

Read in order:
1) PROJECT_BRIEF.md (Sections 3, 7, 8, 12, 14)
2) docs/sprint-3/plan.md
3) docs/sprint-3/progress.md

Tasks owned:
- Task 1: GitHub Actions CI workflow — build + unit + integration tests on every PR and push to main.
- Task 2 (with Ivy): wire Playwright smoke suite into CI. Suite lives in tests/TriTrainer.PlaywrightTests/.

Requirements:
1) CI must run: dotnet build, all four unit/integration test projects, produce test results artifact.
2) Workflow file: .github/workflows/ci.yml
3) TUnit test projects use OutputType: Exe — run with `dotnet run --project ... --configuration Release`, NOT `dotnet test`.
4) Integration tests (TriTrainer.IntegrationTests) require Docker — use a Linux runner with Docker service.
5) Playwright smoke requires Aspire stack up — document pre-requisite clearly; wire if runner supports it, otherwise stub and document in progress.
6) Fix carried-forward issues #2 (TUnit version skew) and #3 (TUnit0015 warnings) if they block CI green.
7) Update docs/sprint-3/progress.md after Task 1 and after Task 2.

Definition of done:
- CI workflow runs green on a test PR against main.
- All 196 baseline tests pass in CI.
- Playwright execution status documented (pass or blocked with reason).
```

### @ai-team-dev

```text
@ai-team-dev
Sprint 3 backend kickoff: personal records hardening and progress analytics.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 13, 14)
2) docs/sprint-3/plan.md
3) docs/sprint-3/progress.md

Tasks owned:
- Task 3: Personal Records API hardening.
- Task 4: Progress analytics API.

Requirements for Task 3:
1) Add GET /v1/records?discipline={d} — filter records by discipline.
2) Add GET /v1/records/personal-best?discipline={d} — return the single best record per discipline.
3) Validate all POST /v1/records payloads: value > 0 required, discipline required.
4) Fix GitHub Issue #2 (TUnit version skew) and Issue #3 (TUnit0015 warnings) in this phase.

Requirements for Task 4:
1) Add GET /v1/progress/summary?weeks={n} — returns per-week compliance %, overall compliance %, current streak (consecutive weeks ≥70% compliance), longest streak.
2) Streak definition: a week counts if overall compliance ≥ 70%.
3) Default weeks = 4; max = 52; return 400 if out of range.
4) All new endpoints follow existing validation and error-semantics patterns.
5) Update docs/sprint-3/progress.md after each task.

Definition of done:
- Tasks 3 and 4 endpoints implemented and manually verifiable via .http file.
- No unresolved blocker defects.
- Handoff notes prepared for frontend and QA.
```

### @ai-team-frontend

```text
@ai-team-frontend
Sprint 3 frontend kickoff: Records page and compliance dashboard widgets.

Read in order:
1) PROJECT_BRIEF.md (Sections 7, 8, 12, 14)
2) docs/sprint-3/plan.md
3) docs/sprint-3/progress.md

Tasks owned:
- Task 5: Records UX.
- Task 6: Compliance dashboard UX.

Requirements for Task 5 (Records page):
1) Discipline tabs: All / Run / Cycle / Swim.
2) Personal-best card per discipline: record value, date set, activity notes snippet.
3) Record list below personal-best: sortable by date, value.
4) PR badge: when a new record is posted, show a "New PR" badge on that row for the session.
5) Trend sparkline per discipline: last 8 records as a mini line chart.

Requirements for Task 6 (Dashboard widgets):
1) Rolling 4-week compliance bar chart: one bar per week, colour-coded (≥90% green / ≥70% yellow / <70% red).
2) Streak widget: current streak (weeks) and longest streak.
3) Next planned session card: discipline emoji, session type, day of week, duration.
4) Use existing design system tokens and CSS from app.css; extend, do not rewrite.
5) Update docs/sprint-3/progress.md after each task.

Definition of done:
- Records page renders correctly with real API data.
- Dashboard shows compliance chart and streak widget.
- No unresolved blocker defects.
```

### @ai-team-qa

```text
@ai-team-qa
Sprint 3 QA kickoff: records and analytics regression coverage plus sprint sign-off.

Read in order:
1) PROJECT_BRIEF.md (Sections 8, 12, 13)
2) docs/sprint-3/plan.md
3) docs/sprint-3/progress.md

Tasks owned:
- Task 7: API regression coverage expansion.
- Task 8 (with Remy): sprint hardening and sign-off.

Requirements for Task 7:
1) Test GET /v1/records?discipline filtering — correct records returned, wrong discipline excluded.
2) Test GET /v1/records/personal-best — returns best value, handles empty dataset.
3) Test GET /v1/progress/summary — correct compliance %, streak calculation, edge cases (0 weeks data, all weeks below threshold, all weeks above).
4) Test weeks parameter validation: weeks < 1 → 400, weeks > 52 → 400, weeks = 4 → 200.
5) Verify Sprint 2 regression baseline still holds (196 tests).
6) File GitHub Issues for any defects with labels: bug + severity:blocker|major|minor.

Exit criteria:
- No blocker defects open.
- CI green (Task 1 complete).
- Sign-off recommendation PASS or CONDITIONAL PASS in docs/qa/sprint-3-signoff.md.
```

### @ai-team-producer checkpoint lane

```text
@ai-team-producer
Run Sprint 3 producer checkpoint lane daily until merge.

Inputs:
- docs/sprint-3/plan.md
- docs/sprint-3/progress.md
- PROJECT_BRIEF.md sections 7, 8, 13
- Open GitHub Issues for sprint scope

Checkpoint workflow:
1) Validate progress against Tasks 1-8 and call out slippage by task number.
2) Detect scope creep; move non-Sprint-3 asks to backlog immediately.
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

1. CI pipeline cannot run integration tests (Docker not available on chosen runner).
   - Mitigation: use `ubuntu-latest` with Docker service. If blocked, document and defer integration CI to a follow-up task; do not block sprint on runner configuration.

2. Playwright smoke suite needs live Aspire stack which may timeout in CI.
   - Mitigation: add health-check wait loop before first assertion; use CI timeout budget of 10 min. If flaky, mark as known flaky and log; do not block merge.

3. Progress summary streak algorithm has ambiguous edge cases (no sessions logged, plan not active).
   - Mitigation: define streak contract explicitly in day-1 decision log before any implementation starts.

4. Records UX sparkline performance — rendering 8 data points per discipline may be slow without a chart library.
   - Mitigation: use inline SVG path for MVP sparkline; defer charting library adoption to Sprint 4.

5. Frontend Tasks 5-6 blocked if backend contracts (Tasks 3-4) are not stable first.
   - Mitigation: backend must produce `.http` file examples and API contract note in progress.md before frontend starts.

## Merge Gates

1. Task completion gate: Tasks 1-8 complete or explicitly deferred with producer approval.
2. CI gate: GitHub Actions workflow runs green on the feature/sprint-3 branch.
3. Playwright gate: smoke suite (19 tests) passes in CI or has documented waiver with owner + ETA.
4. Regression gate: all Sprint 2 baseline tests still pass; new Task 7 tests pass.
5. Defect gate: zero open `severity:blocker` issues; any `severity:major` requires explicit producer + QA acceptance.
6. QA sign-off gate: docs/qa/sprint-3-signoff.md exists with PASS or CONDITIONAL PASS.
7. Documentation gate: PROJECT_BRIEF.md section 8 updated, docs/sprint-3/progress.md current, docs/sprint-3/done.md present.
8. Merge policy gate: regular merge only; no squash, no rebase.

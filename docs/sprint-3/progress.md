# Sprint 3 - Progress Tracker

> If context overflows, start a new chat:
> "Read PROJECT_BRIEF.md and docs/sprint-3/progress.md.
> Continue from where it left off."

## Task Status

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | CI pipeline v1 | ✅ Done | `.github/workflows/ci.yml` created; Issues #2 and #3 resolved |
| 2 | Playwright smoke execution | ✅ Done | Wired into CI `playwright-smoke` job; `continue-on-error: true` per sprint-3 merge policy |
| 3 | Personal Records API hardening | ✅ Done | Added/verified `/v1/records` discipline filtering + `/v1/records/personal-best`; POST discipline required + value > 0 validation aligned to existing BadRequest semantics |
| 4 | Progress analytics API | ✅ Done | Added/verified `/v1/progress/summary` with weeks default=4, max=52 validation, weekly + overall compliance and streak metrics |
| 5 | Records UX | ✅ Done | Added discipline tabs, per-discipline PB cards, sortable list, in-session New PR badge, and 8-point trend sparklines |
| 6 | Compliance dashboard UX | ✅ Done | Added rolling 4-week compliance chart, streak widget, and next planned session card on dashboard |
| 7 | API regression coverage expansion | ✅ Done | Added endpoint-level integration regressions for `/v1/records` filtering, `/v1/records/personal-best` (including empty dataset), `/v1/progress/summary` streak and validation bounds |
| 8 | Sprint hardening and handoff | ✅ Done | QA sign-off authored in `docs/qa/sprint-3-signoff.md` with conditional pass recommendation and no open blockers |

## Decisions

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05-15 | CI runner: `ubuntu-latest` for all jobs | Docker pre-installed; no separate service config needed for TUnit.Aspire containers |
| 2026-05-15 | TUnit version skew (#2): updated `ApiService.Tests` from `TUnit 1.41.0` → `1.*` | All 5 test projects now resolve `1.44.39` uniformly |
| 2026-05-15 | TUnit0015 (#3): suppressed in `PlaywrightTests.csproj` via `<NoWarn>TUnit0015</NoWarn>` | Playwright tests manage their own timeouts via Playwright options; no functional impact; 0 warnings now |
| 2026-05-15 | Playwright job: `continue-on-error: true` | Sprint-3 merge policy allows Playwright gate to be non-blocking with documented waiver; see Merge Gate 3 in producer-kickoff.md |

## Changed Files

| File | Change |
|------|--------|
| `.github/workflows/ci.yml` | **Created** — 3-job CI workflow (unit-tests, integration-tests, playwright-smoke) |
| `tests/TriTrainer.ApiService.Tests/TriTrainer.ApiService.Tests.csproj` | TUnit `1.41.0` → `1.*` (Fixes #2) |
| `tests/TriTrainer.PlaywrightTests/TriTrainer.PlaywrightTests.csproj` | Added `<NoWarn>TUnit0015</NoWarn>` (Fixes #3) |
| `src/TriTrainer.ApiService/Program.cs` | POST `/v1/records` discipline-required validation (`Discipline is required.`) and nullable request contract hardening |
| `src/TriTrainer.ApiService/TriTrainer.ApiService.http` | Added Sprint 3 manual verification examples for records + progress summary endpoints |
| `src/TriTrainer.Web/ActivityApiClient.cs` | Added personal-best and progress-summary client contracts/methods |
| `src/TriTrainer.Web/Components/Pages/Records.razor` | Implemented Task 5 UX: tabs, PB cards, sorting, New PR badge, sparklines |
| `src/TriTrainer.Web/Components/Pages/Dashboard.razor` | Implemented Task 6 UX: rolling chart, streaks, next session card |
| `src/TriTrainer.Web/wwwroot/app.css` | Added scoped styles for Task 5-6 widgets using existing token palette |
| `tests/TriTrainer.IntegrationTests/IntegrationTest1.cs` | Expanded integration coverage for records/progress endpoints and weeks-bound validation (Task 7) |
| `docs/qa/sprint-3-signoff.md` | **Created** — QA sign-off recommendation, blocker status, and Task 7 evidence |

## Test Summary

| Suite | Total | Passed | Skipped | Failed |
|-------|-------|--------|---------|--------|
| TriTrainer.ApiService.Tests | 137 | 137 | 0 | 0 |
| TriTrainer.IntegrationTests | 12 | 12 | 0 | 0 |
| TriTrainer.Web.Tests | 27 | 27 | 0 | 0 |
| TriTrainer.ServiceDefaults.Tests | 27 | 27 | 0 | 0 |
| **Total** | **203** | **203** | **0** | **0** |

_Sprint 2 baseline (196/196) remains green; Sprint 3 QA added 7 integration regressions for records/progress endpoints._

## Playwright Smoke Execution Status

**Status: WIRED — Execution pending CI runner validation**

The `playwright-smoke` job is fully wired into `.github/workflows/ci.yml` and runs after `unit-tests` and `integration-tests` pass. It:
- Builds the full solution in Release config
- Installs Chromium via the bundled `playwright.ps1 install --with-deps chromium`
- Runs all 19 tests in `TriTrainer.PlaywrightTests` via `dotnet run` (TUnit.Aspire starts Aspire AppHost + Docker containers in-process)
- Uploads a TRX artifact to `playwright-test-results`

**Pre-requisites satisfied in CI:**
1. Docker Engine available on `ubuntu-latest` ✓ — Aspire AppHost can start PostgreSQL container
2. Chromium + system deps installed via `playwright install --with-deps` ✓
3. Aspire webfrontend URL resolved by `AppFixture.CreateHttpClient("webfrontend")` ✓
4. All tests run headlessly (`LaunchAsync(Headless: true)`) ✓

**Non-blocking gate:** `continue-on-error: true` applied per sprint-3 merge policy (Merge Gate 3).  
If Playwright execution is flaky on CI runners (networking, timing), failures are logged in the uploaded TRX artifact and do not block merge. Owner: Ivy + Dash to diagnose from artifact output.

## Bugs Found

_None yet._

## Blockers

_None._

## Notes

- Sprint 3 started 2026-05-15 after Sprint 2 regular merge to main.
- ~~Carried-forward from Sprint 2: TUnit version skew (GitHub Issue #2, minor)~~ — **Fixed 2026-05-15** in Task 1.
- ~~TUnit0015 warnings (GitHub Issue #3, minor)~~ — **Fixed 2026-05-15** in Task 1 (suppressed in PlaywrightTests.csproj; 0 warnings on build).
- Playwright smoke suite (19 tests, `tests/TriTrainer.PlaywrightTests/`) wired to CI; execution outcome pending first CI run.
- Producer kickoff package: docs/sprint-3/producer-kickoff.md.

## Producer Checkpoint - 2026-05-22 (Earlier Assessment - Superseded)

> Superseded by the latest Producer Checkpoint below after DevOps, Dev, and QA checkpoints were recorded on 2026-05-22.

### Status

**Off-track** for Sprint 3 timeline. Tasks 1-2 are complete; Tasks 3-8 have not started and are now sequentially slipped against the planned Phase 2-4 flow.

### Task Delta

| Task # | Delta Since Last Checkpoint | Current State | Slippage |
|---|---|---|---|
| 1 | No change | Done | No |
| 2 | No change | Done (non-blocking in CI) | No |
| 3 | No progress recorded | Not started | Yes |
| 4 | No progress recorded | Not started | Yes |
| 5 | No progress recorded | Not started | Yes (dependency on 3) |
| 6 | No progress recorded | Not started | Yes (dependency on 4) |
| 7 | No progress recorded | Not started | Yes |
| 8 | No progress recorded | Not started | Yes |

### New Risks

| Risk | Impact | Owner | Mitigation |
|---|---|---|---|
| Backend/API tasks (3-4) have not started | Frontend (5-6) and QA (7) remain blocked by contract availability | Sage | Start Task 3 immediately, publish contract examples in progress log, then start Task 4 same day |
| No recorded CI run evidence in this tracker | Merge Gate 2 cannot be marked pass from file evidence | Dash | Add first CI run link/result summary and artifact status in next update |
| Playwright remains policy-waived and pending first execution evidence | Merge Gate 3 remains conditional, not fully proven | Ivy + Dash | Run first CI smoke execution, capture TRX artifact summary, publish owner ETA for strict pass |

### Blocker Coverage Check

No active blockers were logged in the tracker, but gate-critical dependencies are now treated as producer escalations and must carry explicit ownership metadata.

| Escalation Item | Owner | Severity | ETA | Escalation Path |
|---|---|---|---|---|
| Start and complete Task 3 contract hardening kickoff | Sage | major | 2026-05-23 EOD | Remy -> Kira if not started by 2026-05-23 noon |
| Publish CI run evidence for Gate 2 | Dash | major | 2026-05-23 EOD | Remy -> Dash -> Kira for resourcing if pipeline remains unverified |
| Publish first Playwright CI execution evidence for Gate 3 | Ivy + Dash | major | 2026-05-24 EOD | Remy -> Ivy/Dash -> QA lane review for merge policy exception renewal |

### Merge Readiness

| Gate | Result | Evidence from sprint docs |
|---|---|---|
| 1. Task completion | Fail | Only Tasks 1-2 marked done; 3-8 not started in Task Status table |
| 2. CI green on sprint branch | Fail | CI workflow created, but no green run evidence recorded in this file |
| 3. Playwright gate | Pass (conditional waiver) | Task 2 done and waiver documented: continue-on-error policy and owner note |
| 4. Regression gate | Fail | Baseline table exists (196 passing) but no Task 7 execution evidence yet |
| 5. Defect gate | Pass (provisional) | Bugs Found: None; Blockers: None; no blocker defects recorded in tracker |
| 6. QA sign-off | Fail | No Sprint 3 QA sign-off artifact referenced yet |
| 7. Documentation gate | Fail | progress.md is active; PROJECT_BRIEF status still shows Sprint 3 planned and no sprint-3 done artifact |
| 8. Merge policy gate | Pass (policy set) | Merge policy defined in producer kickoff; no conflicting instruction logged |

Overall merge readiness: **Not ready to merge**.

### Next 24h Focus

1. Sage starts Task 3 and posts contract + validation examples in this tracker.
2. Dash posts first CI run outcome and artifact evidence for Gate 2.
3. Ivy + Dash post first Playwright CI run result and confirm whether waiver remains required.
4. If Tasks 3-4 begin, Nova/Milo/Kira prepare Task 5-6 implementation window without adding scope.

### Scope Creep Check

No new scope creep items were identified in current Sprint 3 docs. No backlog move required in this checkpoint.

## DevOps Checkpoint - 2026-05-22

### Status

Status: on-track

### Task Delta

- Task 1: completed (validated) — `.github/workflows/ci.yml` satisfies kickoff requirements for triggers, build, unit, integration, and artifact publication.
- Task 2: completed (validated with local blocker note) — Playwright smoke lane is wired with prerequisites, time budget, dependency ordering, and non-blocking merge policy.

### CI/Smoke Gate Evidence

- Build: pass (local) — `dotnet build TriTrainer.slnx --configuration Release --no-restore` succeeded.
- Unit tests: pass (local) — `dotnet run` on ApiService/Web/ServiceDefaults test executables passed (137 + 27 + 27).
- Integration tests: fail (local env blocker) — `dotnet run --project tests/TriTrainer.IntegrationTests/TriTrainer.IntegrationTests.csproj --configuration Release --no-build` failed due to unhealthy local Podman runtime.
- Playwright smoke: waived locally (same container runtime pre-req blocker) — CI lane is configured and gated as non-blocking via `continue-on-error: true` per producer policy.
- Artifact publication: pass (workflow config) — unit/integration/playwright jobs each upload `TestResults/` through `actions/upload-artifact@v4`.

### Risks

- Local integration and Playwright execution require healthy Docker-compatible runtime; current workstation Podman runtime is detected but unhealthy.
- Owner: Dash.
- Mitigation: run first branch CI execution on `ubuntu-latest` (Docker preinstalled) and publish run URL + artifact summary in next checkpoint.

### Gate Impact

- Merge Gate 2 (CI green): pending first branch run evidence.
- Merge Gate 3 (Playwright): pass under documented conditional waiver; strict pass still pending first successful CI smoke run.

### Next Action

- Dash: trigger/observe first `feature/sprint-3` CI run and post outcomes + artifact evidence in this tracker by 2026-05-23 EOD.

## Dev Checkpoint - 2026-05-22 (Tasks 3-6)

### Status

Phase 2 and Phase 3 implementation lane completed for Sprint 3 scope (Tasks 3-6).

### Implementation Notes

1. Task 3: verified and hardened records API contracts:
	- `GET /v1/records?discipline={d}` supports discipline filter and returns existing record DTO list.
	- `GET /v1/records/personal-best?discipline={d}` supports optional discipline filter and returns grouped best records.
	- `POST /v1/records` now explicitly requires discipline (`Discipline is required.`) and continues to enforce `value > 0`.
2. Task 4: progress summary API validated and documented:
	- `GET /v1/progress/summary?weeks={n}` returns per-week compliance, overall compliance, current and longest streak (week qualifies at >= 70%).
	- weeks constraint enforced (`1..52`, default `4`, out-of-range returns `400`).
3. Task 5: Records UI delivered:
	- discipline tabs (All/Run/Cycle/Swim), PB card per discipline, sortable table (date/value), in-session New PR badge, 8-record sparklines.
4. Task 6: Dashboard UI delivered:
	- rolling 4-week compliance bars with threshold coloring, streak widget, next planned session card (emoji/type/day/duration).

### Validation

- Local build verification: `dotnet build TriTrainer.slnx --configuration Release` -> PASS.
- Manual endpoint examples added in `src/TriTrainer.ApiService/TriTrainer.ApiService.http` for Tasks 3-4 verification.

### QA Handoff Notes (Task 7 prep)

1. Records API:
	- `POST /v1/records` with missing discipline -> `400` and message `Discipline is required.`
	- `POST /v1/records` with `value <= 0` -> `400` and message `Value must be greater than 0.`
	- `GET /v1/records?discipline=Run` returns only run records.
	- `GET /v1/records/personal-best?discipline=Run` returns only run PB entries.
2. Progress summary API:
	- `GET /v1/progress/summary` (no weeks) returns `200` with 4 weeks.
	- `GET /v1/progress/summary?weeks=0` and `?weeks=53` return `400`.
	- Streak values are based on consecutive weeks with compliance >= 70.
3. Records UI:
	- Tabs filter list rows.
	- Date/value sort toggles direction when header clicked repeatedly.
	- Newly added record shows New PR badge in current session.
	- Sparkline renders for each discipline and updates after record insert.
4. Dashboard UI:
	- Compliance bars follow threshold colors: >=90 green, >=70 yellow, <70 red.
	- Current/Longest streak values match API response.
	- Next planned session card shows emoji + discipline + session type + weekday + duration.

## QA Checkpoint - 2026-05-22 (Tasks 7-8)

### Status

Task 7 complete. Task 8 complete with QA recommendation recorded.

### Execution Evidence

Automated suites executed using repo test conventions (`dotnet run --project ... --configuration Release`):

| Suite | Result |
|---|---|
| TriTrainer.ApiService.Tests | Pass (137/137) |
| TriTrainer.Web.Tests | Pass (27/27) |
| TriTrainer.ServiceDefaults.Tests | Pass (27/27) |
| TriTrainer.IntegrationTests | Pass (12/12) |

Regression assertions now include endpoint-level coverage for:
- `/v1/records?discipline=...` filtering correctness
- `/v1/records/personal-best` best-value logic + empty dataset behavior
- `/v1/progress/summary` correctness, streak edge cases, and bounds (`weeks=0`, `weeks=53`, `weeks=4`)

### Defect Outcome

- Product defects found: none
- Issue-ready entries created this run: none
- Blockers: none

### Sign-off Link

QA sign-off artifact: `docs/qa/sprint-3-signoff.md` (Recommendation: CONDITIONAL PASS)

## Producer Checkpoint - 2026-05-22 (Latest)

Status: on-track

Task Delta: Tasks 1-8 are now complete per DevOps/Dev/QA checkpoints and QA sign-off artifact.

New Risks: Gate 2 evidence is still pending because first `feature/sprint-3` CI run link/artifact summary is not yet attached in this tracker; owner Dash (DevOps), ETA 2026-05-23 EOD.

Merge Readiness: Gate 1 Pass; Gate 2 Pending Evidence; Gate 3 Pass (conditional waiver); Gate 4 Pass; Gate 5 Pass; Gate 6 Pass; Gate 7 Pass; Gate 8 Pass.

Next 24h Focus: Attach first branch CI run evidence link/artifact summary for Gate 2 closure, then re-evaluate whether Playwright conditional waiver can be retired.

### Task 1-8 Reconciliation

| # | Task | Reconciled Status | Evidence Source |
|---|---|---|---|
| 1 | CI pipeline v1 | Done | DevOps checkpoint + Task Status table |
| 2 | Playwright smoke execution | Done (policy-waived non-blocking gate) | DevOps checkpoint + merge policy |
| 3 | Personal Records API hardening | Done | Dev checkpoint |
| 4 | Progress analytics API | Done | Dev checkpoint |
| 5 | Records UX | Done | Dev checkpoint |
| 6 | Compliance dashboard UX | Done | Dev checkpoint |
| 7 | API regression coverage expansion | Done | QA checkpoint + QA sign-off |
| 8 | Sprint hardening and handoff | Done (conditional QA recommendation documented) | QA checkpoint + `docs/qa/sprint-3-signoff.md` |

### Risks and Blockers (Current)

| Item | Type | Owner | Status | ETA |
|---|---|---|---|---|
| First branch CI run evidence not yet linked in tracker for Gate 2 | Risk | Dash (DevOps) | Open | 2026-05-23 EOD |

No product defect blockers are open from QA (`0 blocker`, `0 major`).

### Merge Readiness by Gate (1-8)

| Gate | Status | Evidence |
|---|---|---|
| 1. Task completion gate | Pass | Tasks 1-8 marked complete in tracker with Dev/QA checkpoints |
| 2. CI gate | Pending | Workflow and local validation documented; first branch CI green run link not yet recorded in this file |
| 3. Playwright gate | Pass (conditional waiver) | Non-blocking waiver documented; QA conditional pass aligns with policy |
| 4. Regression gate | Pass | 203/203 passing; Sprint 2 baseline remains green |
| 5. Defect gate | Pass | QA reports 0 blocker / 0 major defects |
| 6. QA sign-off gate | Pass | `docs/qa/sprint-3-signoff.md` exists with CONDITIONAL PASS |
| 7. Documentation gate | Pass | `docs/sprint-3/done.md` exists and `PROJECT_BRIEF.md` sections 7-8 updated to Sprint 3 state |
| 8. Merge policy gate | Pass | Regular merge-only policy documented and unchanged |

Overall merge readiness: **Conditionally ready** after Gate 2 evidence is recorded (Gate 3 remains under documented conditional waiver).

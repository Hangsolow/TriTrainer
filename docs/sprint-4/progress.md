# Sprint 4 - Progress Tracker

> If context overflows, start a new chat:
> "Read PROJECT_BRIEF.md and docs/sprint-4/progress.md.
> Continue from where it left off."

## Task Status

| # | Task | Status | Notes |
|---|------|--------|-------|
| 1 | CI branch evidence capture and ledger | ⏳ In progress | Local CI-lane validation complete (build/unit/integration green) and evidence schema defined; strict GitHub feature-branch run URL/SHA ledger still pending |
| 2 | Playwright strict evidence closure and waiver workflow | ⏳ In progress | Carry-over gate from Sprint 3; strict pass evidence or approved waiver package required |
| 3 | Aspire startup-health regression expansion | ⬜ Not started | Backlog technical hardening aligned to current distributed maturity |
| 4 | CI artifact quality and failure triage hardening | ⏳ In progress | `.github/workflows/ci.yml` hardened with lane-specific result paths, per-lane manifest, and traceable artifact naming (`ci-<run_id>-<run_attempt>-<lane>`) |
| 5 | Sprint 4 QA and merge readiness package | ⬜ Not started | Final sign-off and handoff gate |

## DevOps Checkpoint - 2026-05-22 (Dash)

- Status: off-track
- Task Delta: Task 4 implemented and locally validated; Task 1 partially complete with local evidence only; Task 1 strict CI evidence blocked by missing GitHub-hosted run capture in this environment
- Risks:
	- Playwright strict smoke remains unstable in local run (4 failures/20 tests); mitigation owner: Ivy with Dash support
	- Strict feature-branch CI evidence cannot be closed until branch push + Actions run is executed and ledgered; mitigation owner: Dash
- Gate Impact:
	- Build: pass (local)
	- Unit tests: pass (local)
	- Integration tests: pass (local)
	- Playwright smoke: fail (local, strict pass not met)
	- Artifact publication: pass (workflow updated for lane-traceable artifacts)
- Next Action: Dash runs and captures first strict GitHub Actions green run on `feature/sprint-4`, then appends run URL/SHA/job matrix to this tracker; Ivy executes Playwright triage and waiver-or-pass closure.

### Task 1 Evidence Package Schema (for strict CI ledger)

Required ledger fields:
1. Branch: `feature/sprint-4`
2. Workflow run URL
3. Run ID and run attempt
4. Commit SHA
5. Trigger type (`push` or `workflow_dispatch`)
6. Job matrix status (`unit-tests`, `integration-tests`, `playwright-smoke`)
7. Artifact inventory
	 - `ci-<run_id>-<run_attempt>-unit`
	 - `ci-<run_id>-<run_attempt>-integration`
	 - `ci-<run_id>-<run_attempt>-playwright`
8. Merge gate mapping (Gates 2, 3, 4, 6 impact)

### DevOps Local Validation Evidence - 2026-05-22

- Build command: `dotnet build TriTrainer.slnx --configuration Release` -> pass
- Unit lane commands (TUnit executable convention):
	- `dotnet run --project tests/TriTrainer.ApiService.Tests/TriTrainer.ApiService.Tests.csproj --configuration Release -- --report-trx --results-directory TestResults/unit/apiservice` -> pass (137/137)
	- `dotnet run --project tests/TriTrainer.Web.Tests/TriTrainer.Web.Tests.csproj --configuration Release -- --report-trx --results-directory TestResults/unit/web` -> pass (27/27)
	- `dotnet run --project tests/TriTrainer.ServiceDefaults.Tests/TriTrainer.ServiceDefaults.Tests.csproj --configuration Release -- --report-trx --results-directory TestResults/unit/servicedefaults` -> pass (27/27)
- Integration lane command:
	- `dotnet run --project tests/TriTrainer.IntegrationTests/TriTrainer.IntegrationTests.csproj --configuration Release -- --report-trx --results-directory TestResults/integration` -> pass (12/12)
- Playwright lane commands:
	- `pwsh tests/TriTrainer.PlaywrightTests/bin/Release/net10.0/playwright.ps1 install --with-deps chromium` -> pass
	- `dotnet run --project tests/TriTrainer.PlaywrightTests/TriTrainer.PlaywrightTests.csproj --configuration Release -- --report-trx --results-directory TestResults/playwright` -> fail (16 passed, 4 failed)

### Strict CI Evidence Action Path (Task 1 closure)

1. Push branch: `git push -u origin feature/sprint-4`
2. Trigger CI via push or `workflow_dispatch` in `.github/workflows/ci.yml`
3. Capture run URL, run ID, run attempt, commit SHA
4. Record job statuses and artifact names in this tracker under Task 1 notes
5. If Playwright remains red, attach Task 2 waiver package (owner, expiry, closure criteria)

## Producer Checkpoint - 2026-05-22

- Status: off-track
- Task delta: completed: none; in-progress: Task 1, Task 2; blocked: Task 3 (blocked by missing Task 1-2 baseline evidence), Task 4 (blocked by missing Task 1-2 baseline evidence), Task 5 (blocked by Tasks 1-4 and QA sign-off artifact)
- New risks: CI strict-green evidence delay (mitigation owner: Dash); Playwright strict-gate instability may force waiver churn (mitigation owner: Ivy); startup-health hardening start delay compresses QA window (mitigation owner: Sage)
- Merge readiness: Gate 1 fail; Gate 2 fail; Gate 3 fail; Gate 4 fail; Gate 5 pass; Gate 6 fail; Gate 7 fail; Gate 8 pass
- Next 24h focus: 1) Dash publishes strict green run URL, commit SHA, and artifact inventory for Task 1. 2) Ivy and Dash publish Playwright strict-pass evidence or approved waiver package with owner, expiry, and closure criteria for Task 2. 3) Sage and Ivy start Task 3 startup-health matrix execution once baseline evidence lands and record first-pass results.

### Slippage Callout

- Task 1: Slipping against day-1 checkpoint expectation because strict feature-branch green evidence is not yet attached.
- Task 2: Slipping because no strict pass evidence or approved waiver package is currently recorded.
- Task 3: Start slippage due to dependency on carry-over gate closure and missing readiness matrix evidence.
- Task 4: Start slippage due to dependency on Task 1-2 evidence artifacts.
- Task 5: Not started and currently blocked pending Tasks 1-4 outcomes and QA sign-off document.

## Decisions

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05-22 | Carry-over gates execute before any non-gate hardening | Sprint 3 closed as conditional pass; merge confidence requires objective closure first |
| 2026-05-22 | Keep Sprint 4 scope to release-gate hardening only | Prevent scope creep and protect one-sprint delivery confidence |

## Changed Files

| File | Change |
|------|--------|
| `.github/workflows/ci.yml` | **Updated** - Added feature branch/manual trigger, lane-specific test result folders, lane evidence manifests, and traceable CI artifact naming for triage |
| `docs/sprint-4/plan.md` | **Created** - Sprint objective, task list, success criteria, out-of-scope |
| `docs/sprint-4/progress.md` | **Created** - Task tracker and decision log |
| `docs/sprint-4/producer-kickoff.md` | **Created** - Producer coordination package |
| `PROJECT_BRIEF.md` | **Updated** - Section 7 and Section 8 for Sprint 4 kickoff |

## Test Summary

| Suite | Total | Passed | Skipped | Failed |
|-------|-------|--------|---------|--------|
| Baseline from Sprint 3 close | 203 | 203 | 0 | 0 |
| Sprint 4 incremental (local DevOps validation) | 223 | 219 | 0 | 4 |
| **Total (current evidence)** | **223** | **219** | **0** | **4** |

## Playwright Smoke Execution Status

Status: CONDITIONAL CARRY-OVER (strict evidence closure pending)

Required for Sprint 4:
1. Attach strict CI smoke evidence (run URL + artifact summary).
2. If strict pass is blocked, publish waiver with owner, expiration date, and closure criteria.
3. Track waiver as open gate until strict evidence is attached.

## Bugs Found

_None yet._

## Blockers

| Blocker | Owner | Severity | ETA | Escalation Path |
|---|---|---|---|---|
| Strict green branch CI evidence for Task 1 is not yet attached in tracker | Dash | major | 2026-05-23 EOD | Escalate to Remy; if still open at ETA, open GitHub issue with `bug` + `severity:major` and trigger same-day triage |
| Playwright strict evidence/waiver package for Task 2 is missing | Ivy | major | 2026-05-23 EOD | Escalate to Remy and QA lead; if unstable after retries, publish timeboxed waiver with closure criteria and expiry |
| Task 3 startup-health execution cannot begin with confidence until Tasks 1-2 baseline evidence is published | Sage | minor | 2026-05-24 midday | Escalate to Remy for re-sequencing if Task 1-2 remain open past ETA |

## Notes

- Sprint 4 started 2026-05-22 after Sprint 3 closed with conditional pass.
- Producer checkpoint cadence: daily, with explicit gate status per Task 1-5.
- Merge is blocked until carry-over gate evidence is explicitly recorded in this tracker.

## Producer Checkpoint Template

### Status

Status: on-track/off-track

### Task Delta

- Task 1:
- Task 2:
- Task 3:
- Task 4:
- Task 5:

### New Risks

| Risk | Impact | Owner | Mitigation |
|---|---|---|---|

### Gate Impact

- Merge Gate 1:
- Merge Gate 2:
- Merge Gate 3:
- Merge Gate 4:
- Merge Gate 5:
- Merge Gate 6:
- Merge Gate 7:
- Merge Gate 8:

### Next 24h Focus

1.
2.
3.

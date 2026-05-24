# Sprint 6 QA Acceptance Matrix

Date: 2026-05-24
Owner: Ivy (QA)
Sprint: 6 - UX Acceleration

## Scope

Primary focus:
- Task 1: Contextual recommendation navigation

Secondary readiness prep:
- Task 2: Dashboard readability and hierarchy pass
- Task 3: Records workflow speed pass
- Task 4: Daily check-in quick path

## Task 1 - Contextual Recommendation Navigation

| Area | Scenario | Expected Result | Status |
|---|---|---|---|
| Dashboard recommendation CTA | Progress-related recommendation with active plan context | Navigates to `progress?planId=<id>&weekStart=<yyyy-MM-dd>` | ✅ Validated by automated tests |
| Dashboard recommendation CTA | Plan-focused recommendation (`activate_plan`, `plan_next_session`, `next_session_gap`) | Navigates to `plans` | ✅ Validated by automated tests |
| Dashboard recommendation CTA | Unknown/empty recommendation code | Safe fallback to `calendar` | ✅ Validated by automated tests |
| Dashboard recommendation CTA | Missing context for progress-related code | Safe fallback to non-context route (no crash) | ✅ Validated by automated tests |
| Manual UX | Browser back/forward after deep-link navigation | Navigation remains stable, no broken state | ⚠ Proxy PASS (headless Playwright); interactive manual pass pending |
| Manual UX | Mobile viewport tap targets and route results | CTA usability and route accuracy remain acceptable | ⚠ Proxy PASS (headless Playwright); interactive manual pass pending |
| Manual UX | Keyboard navigation/enter activation | Accessible CTA navigation works | ⚠ Proxy PASS (headless Playwright); interactive manual pass pending |

## Regression Safety Baseline

| Lane | Command | Expected |
|---|---|---|
| Build | `dotnet build TriTrainer.slnx` | PASS |
| Web tests | `dotnet run --project tests/TriTrainer.Web.Tests/TriTrainer.Web.Tests.csproj --configuration Release` | PASS |
| Integration tests | `dotnet run --project tests/TriTrainer.IntegrationTests/TriTrainer.IntegrationTests.csproj --configuration Release` | PASS |

## Exit Criteria for Task 1 QA

1. Automated build/web/integration lanes are green.
2. Manual deep-link UX checks are completed with no blocker findings.
3. Any issues are filed with severity labels and linked in sprint progress.

## Latest Execution Snapshot (2026-05-24)

- Build: PASS via `dotnet build TriTrainer.slnx`
- Web tests: PASS (41 passed, 0 failed, 0 skipped)
- Integration tests: PASS (19 passed, 0 failed, 0 skipped)
- Playwright tests: PASS (20 passed, 0 failed, 0 skipped)
- Aggregate automated results: 80 passed, 0 failed, 0 skipped
- Blocker verdict: none

Task 2 QA clearance:
- Dashboard readability and hierarchy pass is QA-cleared for automated regression gates.

Task 3 QA clearance:
- Records workflow speed pass is QA-cleared for automated regression gates.

Post-Task-3 execution refresh:
- Web tests: PASS (43 passed, 0 failed, 0 skipped)
- Integration tests: PASS (19 passed, 0 failed, 0 skipped)
- Playwright tests: PASS (20 passed, 0 failed, 0 skipped)
- Aggregate automated results: 82 passed, 0 failed, 0 skipped

Observed non-blocking watch item:
- Integration teardown still emits intermittent aborted-request exception noise in logs during shutdown paths. Continue monitoring for flake risk.

## Task 4 - Daily Check-in Quick Path

| Area | Scenario | Expected Result | Status |
|---|---|---|---|
| Quick-path decision logic | No active goal | CTA routes to `goals` with clear setup label | ✅ Validated by automated tests |
| Quick-path decision logic | Active goal but missing plan context | CTA routes to `plans` with setup continuation label | ✅ Validated by automated tests |
| Quick-path decision logic | Session is due today or earlier | CTA routes to `calendar` for workout logging | ✅ Validated by automated tests |
| Quick-path decision logic | Future session with active plan context | CTA deep-links to `progress?planId=<id>&weekStart=<yyyy-MM-dd>` | ✅ Validated by automated tests |
| Regression safety | Full UI/API/distributed app gates | No failures across build + web + integration + playwright | ✅ Completed |
| Manual UX | Browser back/forward after Daily check-in CTA navigation | Navigation state remains stable and predictable | ⚠ Proxy PASS (headless Playwright); interactive manual pass pending |
| Manual UX | Mobile viewport tap target and readability of Daily check-in card | Control is discoverable and usable without mis-taps | ⚠ Proxy PASS (headless Playwright); interactive manual pass pending |
| Manual UX | Keyboard tab order + Enter/Space activation on Daily check-in CTA | Accessible keyboard operation works end-to-end | ⚠ Proxy PASS (headless Playwright); interactive manual pass pending |

## Execution Snapshot Refresh (2026-05-24)

- Build: PASS via `dotnet build TriTrainer.slnx`
- Web tests: PASS (47 passed, 0 failed, 0 skipped)
- Integration tests: PASS (19 passed, 0 failed, 0 skipped)
- Playwright tests: PASS (23 passed, 0 failed, 0 skipped)
- Aggregate automated results: 89 passed, 0 failed, 0 skipped
- Blocker verdict: no active blocker defects in automated coverage
- QA recommendation status: CONDITIONAL PASS until direct interactive manual UX rows are completed by a human operator

Manual-like UX proxy execution (2026-05-24):
- PASS: `ManualLike_BackForward_StaysStable_AfterDashboardCtaNavigation`
- PASS: `ManualLike_MobileViewport_CtasAreDiscoverableAndTapNavigates`
- PASS: `ManualLike_Keyboard_TabTraversal_EnterActivation_WorksForKeyCtas`
- Evidence: recommendation and daily check-in CTAs matched expected routes; browser history back/forward returned stable dashboard state; mobile tap navigation succeeded with non-zero tap targets; keyboard tab traversal reached CTAs and Enter activated navigation.

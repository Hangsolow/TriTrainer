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
| Manual UX | Browser back/forward after deep-link navigation | Navigation remains stable, no broken state | ⬜ Pending manual pass |
| Manual UX | Mobile viewport tap targets and route results | CTA usability and route accuracy remain acceptable | ⬜ Pending manual pass |
| Manual UX | Keyboard navigation/enter activation | Accessible CTA navigation works | ⬜ Pending manual pass |

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

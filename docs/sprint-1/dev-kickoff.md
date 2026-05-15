# Sprint 1 - Dev Lane Kickoff

Date: 2026-05-15
Owners: Nova + Sage + Milo

## 1) Vertical Slice Lock

Slice chosen: Goal to Plan to Weekly Progress (single athlete).

Decision:
- User can create one training goal (event plus target date plus weekly hours).
- System creates one draft plan shell with 4 weeks and planned sessions.
- User can view a weekly progress page showing planned vs completed minutes by discipline.

Why this slice:
- Covers planning, goals, and progress pillars early.
- Reuses existing activity logging data for completed work, reducing sprint risk.
- Fits current stack without auth or recommendation engine dependency.
- Gives a concrete contract for tasks 1-3 without over-committing full engine logic.

## 2) Domain Model Blueprint (Sprint 1 scope)

Entity decisions (EF Core + PostgreSQL):

- AthleteProfile
  - Id (Guid, PK)
  - DisplayName (string, required, max 120)
  - WeeklyHoursAvailable (decimal(4,1), required)
  - PrimaryEventDate (date, nullable)
  - CreatedAtUtc (timestamp)
- Goal
  - Id (Guid, PK)
  - AthleteId (Guid, FK -> AthleteProfile)
  - GoalType (enum: EventFinish, DisciplinePerformance, Consistency)
  - Discipline (enum nullable: Swim, Bike, Run)
  - TargetValue (decimal nullable)
  - TargetDate (date, required)
  - Status (enum: Draft, Active, Achieved, Archived)
  - CreatedAtUtc (timestamp)
- TrainingPlan
  - Id (Guid, PK)
  - AthleteId (Guid, FK -> AthleteProfile)
  - GoalId (Guid, FK -> Goal, nullable until linked)
  - Name (string, required, max 120)
  - StartDate (date, required)
  - EndDate (date, required)
  - Status (enum: Draft, Active, Completed)
  - CreatedAtUtc (timestamp)
- PlanWeek
  - Id (Guid, PK)
  - PlanId (Guid, FK -> TrainingPlan)
  - WeekIndex (int, required, 1-based)
  - WeekStartDate (date, required)
  - Notes (string nullable, max 500)
- PlannedSession
  - Id (Guid, PK)
  - PlanWeekId (Guid, FK -> PlanWeek)
  - Discipline (enum: Swim, Bike, Run)
  - SessionType (enum: Endurance, Tempo, Intervals, Recovery)
  - PlannedDurationMinutes (int, required, >0)
  - PlannedDistanceKm (decimal nullable)
  - DayOfWeek (tinyint 0-6)
- CompletedActivity (existing Activity reused)
  - Id (Guid, PK)
  - Date (date)
  - Type (enum mapped to Swim/Bike/Run)
  - DurationMinutes (int)
  - Notes (string nullable)
- PersonalRecord
  - Id (Guid, PK)
  - AthleteId (Guid, FK -> AthleteProfile)
  - Discipline (enum: Swim, Bike, Run)
  - Metric (enum: Fastest5k, Fastest10k, LongestRide, LongestSwim)
  - Value (decimal, required)
  - AchievedOn (date, required)
  - SourceActivityId (Guid nullable, FK -> CompletedActivity)

Relationships:
- AthleteProfile 1-to-many Goal.
- AthleteProfile 1-to-many TrainingPlan.
- Goal 1-to-many TrainingPlan (initially one active plan per goal enforced by rule).
- TrainingPlan 1-to-many PlanWeek.
- PlanWeek 1-to-many PlannedSession.
- AthleteProfile 1-to-many PersonalRecord.
- CompletedActivity remains independent for now; linked in aggregates and optional PR source.

Sprint 1 rule constraints:
- One active goal per athlete per GoalType.
- Plan dates must be within goal target date horizon.
- PlannedDurationMinutes must be 15-360.
- No hard delete for Goal or TrainingPlan; use status transitions.

## 3) API Contract Draft (Sprint 1 scope)

Base path: /v1
Style: ASP.NET Minimal API, JSON, validation failures return problem details.

Athlete:
- GET /v1/athlete-profile
  - 200: profile payload
  - 404: not found
- PUT /v1/athlete-profile
  - Request: displayName, weeklyHoursAvailable, primaryEventDate
  - 200: updated profile
  - 400: validation failure

Goals:
- GET /v1/goals?status=Active|Draft|Achieved|Archived
  - 200: array of goals
- POST /v1/goals
  - Request: goalType, discipline, targetValue, targetDate
  - 201: created goal + Location header
  - 400: invalid payload
  - 409: conflicting active goal
- PATCH /v1/goals/{goalId}/status
  - Request: status
  - 200: updated goal
  - 404: not found
  - 409: invalid transition

Plans:
- POST /v1/plans
  - Request: goalId, name, startDate, weekCount (4-16)
  - Behavior: creates TrainingPlan + PlanWeek rows; PlannedSession optional skeleton per week
  - 201: created plan summary
  - 400: invalid payload
  - 404: goal not found
- GET /v1/plans/{planId}
  - 200: plan detail including weeks and planned sessions
  - 404: not found
- GET /v1/plans?status=Draft|Active|Completed
  - 200: array of plan summaries

Progress (weekly):
- GET /v1/progress/weekly?planId={guid}&weekStart=YYYY-MM-DD
  - 200: weekly aggregate
    - weekStartDate
    - disciplines[] with plannedMinutes, completedMinutes, compliancePercent
    - totals with plannedMinutes, completedMinutes, compliancePercent
  - 400: bad query
  - 404: plan/week not found

Personal records (draft contract for Sprint 1):
- GET /v1/records?discipline=Swim|Bike|Run
  - 200: array
- POST /v1/records
  - Request: discipline, metric, value, achievedOn, sourceActivityId (optional)
  - 201: created
  - 400: validation failure

Payload shape examples (canonical):
- Goal create: goalType, discipline, targetValue, targetDate.
- Plan create: goalId, name, startDate, weekCount.
- Weekly progress response: planId, weekStartDate, disciplines[], totals.

## 4) UI Information Architecture (Sprint 1 scope)

Navigation replacing calendar-only flow:

Primary nav:
- Dashboard
- Plans
- Goals
- Records
- Activities (legacy view retained, no longer home)

Pages and purpose:
- Dashboard
  - Default landing page
  - Shows active goal, active plan week, weekly compliance card
  - CTA: Create Goal or Create Plan
- Goals
  - List plus status filters
  - Create Goal panel
  - Goal detail card with linked plan count and status actions
- Plans
  - Plan list (Draft/Active/Completed)
  - Plan detail with week tabs and planned session rows
  - Week selector links to Weekly Progress view
- Weekly Progress (plan scoped)
  - Planned vs completed minutes by discipline
  - Compliance percentage and variance indicators
  - Pulls completed data from existing activities
- Records
  - PR list by discipline
  - Add PR entry
- Activities
  - Existing calendar page moved under secondary nav path for continuity

Flow (happy path):
- Dashboard -> Goals -> Create Goal -> Plans -> Create Plan from Goal -> Plan Detail Week 1 -> Weekly Progress -> Activities (optional logging) -> Weekly Progress refresh.

IA decisions:
- Calendar is no longer root route.
- New views are plan-first and goal-driven.
- Existing activity CRUD remains available but positioned as supporting input.

## 5) First 48h Task Breakdown

Sequence and ownership for tasks 1-3:

1. Task 1: Domain model blueprint freeze
- Owner: Sage
- Est: 4h
- Output: entity list, fields, relations, invariants, migration approach note

2. Cross-check domain against current Activity model
- Owner: Sage
- Est: 1.5h
- Output: compatibility notes, no-break assumptions for existing data

3. Task 2: API contract draft v1
- Owner: Sage
- Est: 4h
- Output: endpoint matrix, request and response DTO schema, status code matrix

4. Contract review for UI fit
- Owner: Nova
- Est: 1.5h
- Output: API-to-screen mapping and missing field list

5. Task 3: UI information architecture map
- Owners: Nova (structure), Milo (visual hierarchy constraints)
- Est: Nova 3h, Milo 2h
- Output: page map, route decisions, navigation flow, component zones

6. IA visual baseline guardrails (handoff into Task 4)
- Owner: Milo
- Est: 2h
- Output: spacing and type hierarchy constraints tied to IA blocks

7. Sprint packet consolidation and sign-off update
- Owners: Nova + Sage + Milo
- Est: 1h
- Output: finalized kickoff packet and decisions log in sprint docs

Total estimated effort (48h window):
- Sage: 9.5h
- Nova: 5.5h
- Milo: 4h
- Team total: 19h

## 6) Acceptance Criteria for Tasks 1-3

Task 1 complete when:
- Domain includes AthleteProfile, Goal, TrainingPlan, PlanWeek, PlannedSession, PersonalRecord, and Activity compatibility note.
- Every entity has key fields, PK/FK direction, and validation constraints.
- Relationships are explicit and support the chosen vertical slice.

Task 2 complete when:
- Contract includes endpoints for goals, plans, weekly progress, and records draft.
- Each endpoint defines method, route, request shape, response shape, and status expectations.
- Error semantics are defined for 400, 404, and 409 where relevant.
- Contract supports the UI flow in Task 3 with no missing required field.

Task 3 complete when:
- IA defines primary navigation and page purpose beyond calendar-only PoC.
- Flow from goal creation to weekly progress is explicit and route-ready.
- Activities calendar is repositioned as supporting workflow, not home.
- IA is implementable in Blazor Server page and component structure without extra backend assumptions.

## 7) Open Questions (blocking only)

1. Athlete identity scope for Sprint 1: assume exactly one local athlete profile in dev, or require athleteId in every route now?
2. Time boundary rule: week start fixed to Monday for all progress calculations, or derived from athlete preference?
3. Plan generation minimum: when creating a plan, should session skeleton auto-generate by default, or can week/session rows start empty?

# TriTrainer Brainstorm - Phase 1 Free Ideation

Date: 2026-05-15
Facilitator: Remy (Producer)

## Kira (Product Designer)
1. Adaptive Build Engine
- A weekly plan that adjusts based on completion rate, fatigue feedback, and available hours.
- Keeps athletes realistic and reduces guilt-driven overtraining.

2. Goal Ladder
- Every long-term race goal is broken into monthly and weekly micro-goals with visible checkpoints.
- Makes progress feel concrete instead of abstract.

3. Confidence Meter
- A readiness score that combines recent consistency, discipline balance, and PR trend.
- Helps users decide if they should push, maintain, or recover.

## Milo (Visual/Art Director)
1. Triad Canvas UI
- Three discipline lanes (swim, bike, run) as a living timeline with distinct visual rhythms.
- A non-generic interface that feels athletic and intentional.

2. Momentum Ring System
- Circular progress motifs for goals and training load that animate on completion.
- Gives emotional payoff for consistency.

3. Effort Heat Atlas
- Calendar heat map with intensity and completion overlays.
- Makes patterns and missed blocks obvious in seconds.

## Nova (Frontend Engineer)
1. Modular Training Workspace
- Split UI into reusable modules: Today, Week Plan, Goal Tracker, PR Board.
- Allows incremental shipping and low-risk redesign.

2. Plan Drafting Wizard
- Guided multi-step form for creating plans from race date, availability, and focus areas.
- Reduces user setup friction and keeps backend payloads structured.

3. Offline-friendly Session Logging
- Cache session drafts client-side and sync when connection recovers.
- Prevents data loss during mobile gym/pool usage.

## Sage (Backend Engineer)
1. Domain Rebuild with Bounded Contexts
- Separate contexts for Planning, Goals, Activity Log, and Personal Records.
- Enables clear API boundaries and less coupling than the current PoC.

2. Goal Evaluation Engine
- Server computes target progress status (on track, at risk, behind).
- Keeps business logic centralized and testable in C#.

3. Structured Event Log
- Domain events for plan updates, milestone hits, and PR changes.
- Improves traceability and future analytics capabilities.

## Remy (Producer)
1. Vertical Slice First
- Ship one full flow: create plan -> complete sessions -> see goal/PR impact.
- Avoid broad rewrite paralysis.

2. Two-Layer Scope
- Layer A must ship (core planning + goals + PR tracking).
- Layer B can slip (advanced analytics and social features).

3. Issue-driven Sprinting
- Every major risk captured as GitHub issue before coding starts.
- Prevents invisible scope drift.

## Ivy (QA Engineer)
1. Reliability-first Workflow
- Introduce acceptance checks per flow before visual polish.
- Catch regressions early while architecture is changing.

2. Edge-case Suite for Training Data
- Test leap years, timezone shifts, backfilled sessions, and race date changes.
- Prevents silent data corruption in plan timelines.

3. QA Gates on Merge
- No merge without a completed sprint sign-off and blocker review.
- Keeps main branch stable during heavy refactor.

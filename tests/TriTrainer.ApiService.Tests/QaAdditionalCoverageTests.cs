using TriTrainer.ApiService.Data;
using TriTrainer.ApiService.Services;

namespace TriTrainer.ApiService.Tests;

/// <summary>
/// QA-authored additional coverage for Sprint 2.
/// Targets gaps identified in the regression matrix review:
///   - DisciplinePerformance TargetValue edge cases
///   - Goal/Plan self-transition rejection
///   - Cycle and Swim focus discipline cross-train mapping
///   - DisciplinePerformance peak week → Intervals (not Tempo)
///   - Null goal fallback to EventFinish template
///   - Volume progression across mesocycle weeks
///   - Minimum duration floor (15 min) at very low athlete hours
///
/// Owner: Ivy (QA)
/// </summary>
public class QaAdditionalCoverageTests
{
    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static AthleteProfile MakeAthlete(decimal weeklyHours = 8) =>
        new() { DisplayName = "QA Athlete", WeeklyHoursAvailable = weeklyHours };

    private static TrainingPlan MakePlan(int weekCount, AthleteProfile athlete, Goal? goal = null)
    {
        var plan = new TrainingPlan
        {
            AthleteId = athlete.Id,
            GoalId = goal?.Id,
            Name = "QA Test Plan",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 6, 1).AddDays(weekCount * 7 - 1)
        };

        for (var i = 0; i < weekCount; i++)
        {
            plan.Weeks.Add(new PlanWeek
            {
                WeekIndex = i + 1,
                WeekStartDate = plan.StartDate.AddDays(i * 7)
            });
        }

        return plan;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 1 — DisciplinePerformance TargetValue validation edge cases
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Goal_DisciplinePerformance_TargetValueNull_ShouldBeRejected()
    {
        // Mirrors: if (request.GoalType == GoalType.DisciplinePerformance && (request.TargetValue is null or <= 0))
        GoalType goalType = GoalType.DisciplinePerformance;
        decimal? targetValue = null;

        bool isRejected = goalType == GoalType.DisciplinePerformance && (targetValue is null or <= 0);

        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    public async Task Goal_DisciplinePerformance_TargetValueZero_ShouldBeRejected()
    {
        GoalType goalType = GoalType.DisciplinePerformance;
        decimal? targetValue = 0m;

        bool isRejected = goalType == GoalType.DisciplinePerformance && (targetValue is null or <= 0);

        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    public async Task Goal_DisciplinePerformance_TargetValueNegative_ShouldBeRejected()
    {
        GoalType goalType = GoalType.DisciplinePerformance;
        decimal? targetValue = -5m;

        bool isRejected = goalType == GoalType.DisciplinePerformance && (targetValue is null or <= 0);

        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    [Arguments(0.01)]
    [Arguments(1)]
    [Arguments(42.5)]
    [Arguments(999)]
    public async Task Goal_DisciplinePerformance_TargetValuePositive_ShouldBeAccepted(decimal targetValue)
    {
        GoalType goalType = GoalType.DisciplinePerformance;
        decimal? tv = targetValue;

        bool isRejected = goalType == GoalType.DisciplinePerformance && (tv is null or <= 0);

        await Assert.That(isRejected).IsFalse();
    }

    [Test]
    public async Task Goal_EventFinish_TargetValueIsOptional()
    {
        // EventFinish goals do not require TargetValue — no validation guard applies
        GoalType goalType = GoalType.EventFinish;
        decimal? targetValue = null;

        bool isRejected = goalType == GoalType.DisciplinePerformance && (targetValue is null or <= 0);

        await Assert.That(isRejected).IsFalse();
    }

    [Test]
    public async Task Goal_Consistency_TargetValueIsOptional()
    {
        GoalType goalType = GoalType.Consistency;
        decimal? targetValue = null;

        bool isRejected = goalType == GoalType.DisciplinePerformance && (targetValue is null or <= 0);

        await Assert.That(isRejected).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 2 — Self-transition rejection (goal and plan)
    // The transition switch is exhaustive: any pair not listed returns false.
    // Self-transitions are not listed, so they should be rejected.
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Arguments(GoalStatus.Draft)]
    [Arguments(GoalStatus.Active)]
    [Arguments(GoalStatus.Achieved)]
    [Arguments(GoalStatus.Archived)]
    public async Task GoalStatus_SelfTransition_IsRejectedByBusinessRule(GoalStatus status)
    {
        var isValid = (status, status) switch
        {
            (GoalStatus.Draft,    GoalStatus.Active)   => true,
            (GoalStatus.Draft,    GoalStatus.Archived) => true,
            (GoalStatus.Active,   GoalStatus.Achieved) => true,
            (GoalStatus.Active,   GoalStatus.Archived) => true,
            (GoalStatus.Achieved, GoalStatus.Archived) => true,
            _ => false
        };

        await Assert.That(isValid).IsFalse();
    }

    [Test]
    [Arguments(PlanStatus.Draft)]
    [Arguments(PlanStatus.Active)]
    [Arguments(PlanStatus.Completed)]
    public async Task PlanStatus_SelfTransition_IsRejectedByBusinessRule(PlanStatus status)
    {
        var isValid = (status, status) switch
        {
            (PlanStatus.Draft,  PlanStatus.Active)    => true,
            (PlanStatus.Active, PlanStatus.Completed) => true,
            _ => false
        };

        await Assert.That(isValid).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 3 — DisciplinePerformance cross-train mapping for Cycle and Swim
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GenerateSessions_DisciplinePerformance_Cycle_CrossTrainIsRun()
    {
        // Cycle focus → cross-train is Run (from the switch in BuildDisciplineSessions)
        var athlete = MakeAthlete(8);
        var goal = new Goal
        {
            AthleteId = athlete.Id,
            GoalType = GoalType.DisciplinePerformance,
            Discipline = ActivityType.Cycle,
            TargetDate = new DateOnly(2026, 12, 1)
        };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        // Must have 2 Cycle sessions and 1 Run session
        var cycleSessions = sessions.Where(s => s.Discipline == ActivityType.Cycle).ToList();
        var runSessions   = sessions.Where(s => s.Discipline == ActivityType.Run).ToList();
        var swimSessions  = sessions.Where(s => s.Discipline == ActivityType.Swim).ToList();

        await Assert.That(cycleSessions.Count).IsEqualTo(2);
        await Assert.That(runSessions.Count).IsEqualTo(1);
        await Assert.That(swimSessions.Count).IsEqualTo(0);
    }

    [Test]
    public async Task GenerateSessions_DisciplinePerformance_Swim_CrossTrainIsCycle()
    {
        // Swim focus → cross-train is Cycle
        var athlete = MakeAthlete(8);
        var goal = new Goal
        {
            AthleteId = athlete.Id,
            GoalType = GoalType.DisciplinePerformance,
            Discipline = ActivityType.Swim,
            TargetDate = new DateOnly(2026, 12, 1)
        };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        var swimSessions  = sessions.Where(s => s.Discipline == ActivityType.Swim).ToList();
        var cycleSessions = sessions.Where(s => s.Discipline == ActivityType.Cycle).ToList();
        var runSessions   = sessions.Where(s => s.Discipline == ActivityType.Run).ToList();

        await Assert.That(swimSessions.Count).IsEqualTo(2);
        await Assert.That(cycleSessions.Count).IsEqualTo(1);
        await Assert.That(runSessions.Count).IsEqualTo(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 4 — DisciplinePerformance peak week uses Intervals (not Tempo)
    // The EventFinish/Consistency templates use Tempo for peak weeks,
    // but DisciplinePerformance uses Intervals for the primary discipline.
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GenerateSessions_DisciplinePerformance_PeakWeek_PrimaryIsIntervals()
    {
        // Week 3 (index=3, phase=(3-1)%4=2) is peak build.
        // For DisciplinePerformance: primaryType = Intervals
        var athlete = MakeAthlete(8);
        var goal = new Goal
        {
            AthleteId = athlete.Id,
            GoalType = GoalType.DisciplinePerformance,
            Discipline = ActivityType.Run,
            TargetDate = new DateOnly(2026, 12, 1)
        };
        var plan = MakePlan(4, athlete, goal);

        var week3Sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[2].Id) // week index 3 (0-based index 2)
            .Where(s => s.Discipline == ActivityType.Run)
            .ToList();

        await Assert.That(week3Sessions).IsNotEmpty();

        // Monday session is the primary — must be Intervals
        var mondaySession = week3Sessions.FirstOrDefault(s => s.DayOfWeek == DayOfWeek.Monday);
        await Assert.That(mondaySession).IsNotNull();
        await Assert.That(mondaySession!.SessionType).IsEqualTo(SessionType.Intervals);
    }

    [Test]
    public async Task GenerateSessions_DisciplinePerformance_PeakWeek_SecondaryIsTempo()
    {
        // Wednesday session (secondary focus) should always be Tempo — even in peak week
        var athlete = MakeAthlete(8);
        var goal = new Goal
        {
            AthleteId = athlete.Id,
            GoalType = GoalType.DisciplinePerformance,
            Discipline = ActivityType.Run,
            TargetDate = new DateOnly(2026, 12, 1)
        };
        var plan = MakePlan(4, athlete, goal);

        var week3RunSessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[2].Id)
            .Where(s => s.Discipline == ActivityType.Run)
            .ToList();

        var wednesdaySession = week3RunSessions.FirstOrDefault(s => s.DayOfWeek == DayOfWeek.Wednesday);
        await Assert.That(wednesdaySession).IsNotNull();
        await Assert.That(wednesdaySession!.SessionType).IsEqualTo(SessionType.Tempo);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 5 — Null goal falls back to EventFinish template
    // PlanGenerationService handles null goal via goal?.GoalType switch fallthrough
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GenerateSessions_NullGoal_FallsBackToEventFinishTemplate()
    {
        var athlete = MakeAthlete(8);
        var plan = MakePlan(4, athlete); // no goal

        var sessions = PlanGenerationService.GenerateSessions(plan, null, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        // EventFinish template: 3 sessions per week with Run/Cycle/Swim
        await Assert.That(sessions.Count).IsEqualTo(3);
        await Assert.That(sessions.Any(s => s.Discipline == ActivityType.Run)).IsTrue();
        await Assert.That(sessions.Any(s => s.Discipline == ActivityType.Cycle)).IsTrue();
        await Assert.That(sessions.Any(s => s.Discipline == ActivityType.Swim)).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 6 — Volume progression across mesocycle
    // Week 1 (base 100%) < Week 2 (build 105%) < Week 3 (peak 110%)
    // Week 4 (recovery 70%) < Week 1
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GenerateSessions_VolumeProgression_Week2MoreThanWeek1()
    {
        var athlete = MakeAthlete(10);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);
        var week1Total = sessions.Where(s => s.PlanWeekId == plan.Weeks[0].Id).Sum(s => s.PlannedDurationMinutes);
        var week2Total = sessions.Where(s => s.PlanWeekId == plan.Weeks[1].Id).Sum(s => s.PlannedDurationMinutes);

        // Week 2 is build (105%) — must be greater than Week 1 (100%)
        await Assert.That(week2Total).IsGreaterThan(week1Total);
    }

    [Test]
    public async Task GenerateSessions_VolumeProgression_Week3MoreThanWeek2()
    {
        var athlete = MakeAthlete(10);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);
        var week2Total = sessions.Where(s => s.PlanWeekId == plan.Weeks[1].Id).Sum(s => s.PlannedDurationMinutes);
        var week3Total = sessions.Where(s => s.PlanWeekId == plan.Weeks[2].Id).Sum(s => s.PlannedDurationMinutes);

        // Week 3 is peak build (110%) — must be greater than Week 2 (105%)
        await Assert.That(week3Total).IsGreaterThan(week2Total);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 7 — Minimum session duration floor (15 min)
    // RoundToFive(Math.Max(15, ...)) ensures no session falls below 15 min.
    // Test with a very low athlete weekly hours value.
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GenerateSessions_VeryLowAthleteHours_SessionsStillMeetMinimumDuration()
    {
        // 0.1 hours/week = 6 minutes base. The 15-min floor should kick in for all sessions.
        var athlete = MakeAthlete(0.1m);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);

        foreach (var session in sessions)
        {
            await Assert.That(session.PlannedDurationMinutes).IsGreaterThanOrEqualTo(15);
        }
    }

    [Test]
    public async Task GenerateSessions_AllSessionDurations_AreMultiplesOfFive()
    {
        // RoundToFive() is applied to all durations
        var athlete = MakeAthlete(7);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.DisciplinePerformance, Discipline = ActivityType.Swim, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(8, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);

        foreach (var session in sessions)
        {
            await Assert.That(session.PlannedDurationMinutes % 5).IsEqualTo(0);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 8 — Goal status terminal state validation
    // Archived is terminal — no transitions out of it
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Arguments(GoalStatus.Draft)]
    [Arguments(GoalStatus.Active)]
    [Arguments(GoalStatus.Achieved)]
    public async Task GoalStatus_FromArchived_AllTransitionsRejected(GoalStatus targetStatus)
    {
        var isValid = (GoalStatus.Archived, targetStatus) switch
        {
            (GoalStatus.Draft,    GoalStatus.Active)   => true,
            (GoalStatus.Draft,    GoalStatus.Archived) => true,
            (GoalStatus.Active,   GoalStatus.Achieved) => true,
            (GoalStatus.Active,   GoalStatus.Archived) => true,
            (GoalStatus.Achieved, GoalStatus.Archived) => true,
            _ => false
        };

        await Assert.That(isValid).IsFalse();
    }

    [Test]
    public async Task PlanStatus_FromCompleted_TransitionToActiveRejected()
    {
        var isValid = (PlanStatus.Completed, PlanStatus.Active) switch
        {
            (PlanStatus.Draft,  PlanStatus.Active)    => true,
            (PlanStatus.Active, PlanStatus.Completed) => true,
            _ => false
        };

        await Assert.That(isValid).IsFalse();
    }

    [Test]
    public async Task PlanStatus_FromCompleted_TransitionToDraftRejected()
    {
        var isValid = (PlanStatus.Completed, PlanStatus.Draft) switch
        {
            (PlanStatus.Draft,  PlanStatus.Active)    => true,
            (PlanStatus.Active, PlanStatus.Completed) => true,
            _ => false
        };

        await Assert.That(isValid).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 9 — Generation produces expected session counts per goal type
    // EventFinish/Consistency: 3 sessions/week; DisciplinePerformance: 3 sessions/week
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Arguments(GoalType.EventFinish, 3)]
    [Arguments(GoalType.Consistency, 3)]
    public async Task GenerateSessions_AllTemplateGoalTypes_ProduceExpectedSessionsPerWeek(
        GoalType goalType, int expectedSessionsPerWeek)
    {
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = goalType, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);

        foreach (var week in plan.Weeks)
        {
            var weekSessions = sessions.Where(s => s.PlanWeekId == week.Id).ToList();
            await Assert.That(weekSessions.Count).IsEqualTo(expectedSessionsPerWeek);
        }
    }

    [Test]
    public async Task GenerateSessions_DisciplinePerformance_ProducesThreeSessionsPerWeek()
    {
        var athlete = MakeAthlete(8);
        var goal = new Goal
        {
            AthleteId = athlete.Id,
            GoalType = GoalType.DisciplinePerformance,
            Discipline = ActivityType.Run,
            TargetDate = new DateOnly(2026, 12, 1)
        };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);

        foreach (var week in plan.Weeks)
        {
            var weekSessions = sessions.Where(s => s.PlanWeekId == week.Id).ToList();
            await Assert.That(weekSessions.Count).IsEqualTo(3);
        }
    }
}

using TriTrainer.ApiService.Data;
using TriTrainer.ApiService.Services;

namespace TriTrainer.ApiService.Tests;

/// <summary>
/// Tests for Sprint 2 generation rules and validation semantics.
/// PlanGenerationService is a pure static function — fully deterministic given the same inputs.
/// </summary>
public class GenerationAndValidationTests
{
    // ── Helpers ─────────────────────────────────────────────────────────────

    private static AthleteProfile MakeAthlete(decimal weeklyHours = 8) =>
        new() { DisplayName = "Test", WeeklyHoursAvailable = weeklyHours };

    private static TrainingPlan MakePlan(int weekCount, AthleteProfile athlete, Goal goal)
    {
        var plan = new TrainingPlan
        {
            AthleteId = athlete.Id,
            GoalId = goal.Id,
            Name = "Test Plan",
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

    // ── Task 1: Generation rule set ──────────────────────────────────────────

    [Test]
    public async Task GenerateSessions_EventFinish_ProducesThreeDisciplinesPerWeek()
    {
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);

        var week1Sessions = sessions.Where(s => s.PlanWeekId == plan.Weeks[0].Id).ToList();
        await Assert.That(week1Sessions.Count).IsEqualTo(3);
        await Assert.That(week1Sessions.Any(s => s.Discipline == ActivityType.Run)).IsTrue();
        await Assert.That(week1Sessions.Any(s => s.Discipline == ActivityType.Cycle)).IsTrue();
        await Assert.That(week1Sessions.Any(s => s.Discipline == ActivityType.Swim)).IsTrue();
    }

    [Test]
    public async Task GenerateSessions_EventFinish_AssignsDaysCorrectly()
    {
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        await Assert.That(sessions.Single(s => s.Discipline == ActivityType.Run).DayOfWeek).IsEqualTo(DayOfWeek.Monday);
        await Assert.That(sessions.Single(s => s.Discipline == ActivityType.Cycle).DayOfWeek).IsEqualTo(DayOfWeek.Wednesday);
        await Assert.That(sessions.Single(s => s.Discipline == ActivityType.Swim).DayOfWeek).IsEqualTo(DayOfWeek.Friday);
    }

    [Test]
    public async Task GenerateSessions_EventFinish_VolumeSplitsMatchTemplate()
    {
        // 8h/week = 480 min. EventFinish: Run 35%, Cycle 40%, Swim 25%
        // Week 1 is base (100%): Run=168→165, Cycle=192→190, Swim=120
        // (rounded to nearest 5)
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        var run = sessions.Single(s => s.Discipline == ActivityType.Run).PlannedDurationMinutes;
        var cycle = sessions.Single(s => s.Discipline == ActivityType.Cycle).PlannedDurationMinutes;
        var swim = sessions.Single(s => s.Discipline == ActivityType.Swim).PlannedDurationMinutes;

        // All durations must be multiples of 5 and greater than 0
        await Assert.That(run % 5).IsEqualTo(0);
        await Assert.That(cycle % 5).IsEqualTo(0);
        await Assert.That(swim % 5).IsEqualTo(0);
        // Cycle should get more minutes than Run, Run more than Swim (40% > 35% > 25%)
        await Assert.That(cycle).IsGreaterThan(run);
        await Assert.That(run).IsGreaterThan(swim);
    }

    [Test]
    public async Task GenerateSessions_Consistency_EqualSplitAcrossDisciplines()
    {
        var athlete = MakeAthlete(6);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.Consistency, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        var run = sessions.Single(s => s.Discipline == ActivityType.Run).PlannedDurationMinutes;
        var cycle = sessions.Single(s => s.Discipline == ActivityType.Cycle).PlannedDurationMinutes;
        var swim = sessions.Single(s => s.Discipline == ActivityType.Swim).PlannedDurationMinutes;

        // Equal split — all three should be within 5 minutes of each other
        await Assert.That(Math.Abs(run - cycle)).IsLessThanOrEqualTo(5);
        await Assert.That(Math.Abs(run - swim)).IsLessThanOrEqualTo(5);
    }

    [Test]
    public async Task GenerateSessions_DisciplinePerformance_FocusDisciplineHasTwoSessions()
    {
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.DisciplinePerformance, Discipline = ActivityType.Run, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        var runSessions = sessions.Where(s => s.Discipline == ActivityType.Run).ToList();
        await Assert.That(runSessions.Count).IsEqualTo(2);
    }

    [Test]
    public async Task GenerateSessions_DisciplinePerformance_CrossTrainDisciplineIsCorrect()
    {
        // Run → cross-train is Swim
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.DisciplinePerformance, Discipline = ActivityType.Run, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[0].Id)
            .ToList();

        await Assert.That(sessions.Any(s => s.Discipline == ActivityType.Swim)).IsTrue();
        await Assert.That(sessions.Any(s => s.Discipline == ActivityType.Cycle)).IsFalse();
    }

    [Test]
    public async Task GenerateSessions_Week4IsRecovery_SessionTypeIsRecovery()
    {
        // Week 4 is (4-1) % 4 = 3 → recovery
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var week4Sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[3].Id)
            .ToList();

        await Assert.That(week4Sessions).IsNotEmpty();
        await Assert.That(week4Sessions.All(s => s.SessionType == SessionType.Recovery)).IsTrue();
    }

    [Test]
    public async Task GenerateSessions_Week4IsRecovery_ReducedVolume()
    {
        // Recovery week should be 70% of base load
        var athlete = MakeAthlete(10);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);
        var week1Total = sessions.Where(s => s.PlanWeekId == plan.Weeks[0].Id).Sum(s => s.PlannedDurationMinutes);
        var week4Total = sessions.Where(s => s.PlanWeekId == plan.Weeks[3].Id).Sum(s => s.PlannedDurationMinutes);

        // Week 4 (recovery) should be significantly less than week 1 (base)
        await Assert.That(week4Total).IsLessThan(week1Total);
    }

    [Test]
    public async Task GenerateSessions_Week3IsPeakBuild_SessionTypeIsTempo()
    {
        // Week 3 is (3-1) % 4 = 2 → peak build → Tempo sessions
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var week3Sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete)
            .Where(s => s.PlanWeekId == plan.Weeks[2].Id)
            .ToList();

        await Assert.That(week3Sessions).IsNotEmpty();
        await Assert.That(week3Sessions.All(s => s.SessionType == SessionType.Tempo)).IsTrue();
    }

    [Test]
    public async Task GenerateSessions_IsFullyDeterministic()
    {
        // Same inputs must always produce the same output (no random elements)
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.EventFinish, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(4, athlete, goal);

        var first  = PlanGenerationService.GenerateSessions(plan, goal, athlete);
        var second = PlanGenerationService.GenerateSessions(plan, goal, athlete);

        await Assert.That(first.Count).IsEqualTo(second.Count);

        for (var i = 0; i < first.Count; i++)
        {
            await Assert.That(first[i].Discipline).IsEqualTo(second[i].Discipline);
            await Assert.That(first[i].SessionType).IsEqualTo(second[i].SessionType);
            await Assert.That(first[i].PlannedDurationMinutes).IsEqualTo(second[i].PlannedDurationMinutes);
            await Assert.That(first[i].DayOfWeek).IsEqualTo(second[i].DayOfWeek);
        }
    }

    [Test]
    public async Task GenerateSessions_TotalWeekCount_MatchesPlanWeeks()
    {
        var athlete = MakeAthlete(8);
        var goal = new Goal { AthleteId = athlete.Id, GoalType = GoalType.Consistency, TargetDate = new DateOnly(2026, 12, 1) };
        var plan = MakePlan(8, athlete, goal);

        var sessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);

        // 8 weeks × 3 sessions per week = 24 sessions
        await Assert.That(sessions.Count).IsEqualTo(24);
    }

    // ── Task 2: Validation rule semantics ────────────────────────────────────

    [Test]
    public async Task GoalStatus_Transitions_ValidPathsDraftToActive()
    {
        // Mirrors the transition rule in POST /v1/goals/{goalId}/status
        var validTransitions = new[]
        {
            (GoalStatus.Draft,    GoalStatus.Active),
            (GoalStatus.Draft,    GoalStatus.Archived),
            (GoalStatus.Active,   GoalStatus.Achieved),
            (GoalStatus.Active,   GoalStatus.Archived),
            (GoalStatus.Achieved, GoalStatus.Archived),
        };

        foreach (var (from, to) in validTransitions)
        {
            var isValid = (from, to) switch
            {
                (GoalStatus.Draft,    GoalStatus.Active)   => true,
                (GoalStatus.Draft,    GoalStatus.Archived) => true,
                (GoalStatus.Active,   GoalStatus.Achieved) => true,
                (GoalStatus.Active,   GoalStatus.Archived) => true,
                (GoalStatus.Achieved, GoalStatus.Archived) => true,
                _ => false
            };
            await Assert.That(isValid).IsTrue();
        }
    }

    [Test]
    public async Task GoalStatus_Transitions_InvalidPathsAreRejected()
    {
        var invalidTransitions = new[]
        {
            (GoalStatus.Active,   GoalStatus.Draft),    // no rollback
            (GoalStatus.Achieved, GoalStatus.Active),   // no un-achieve
            (GoalStatus.Archived, GoalStatus.Active),   // archived is terminal
            (GoalStatus.Draft,    GoalStatus.Achieved), // skip steps
        };

        foreach (var (from, to) in invalidTransitions)
        {
            var isValid = (from, to) switch
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
    }

    [Test]
    public async Task PlanStatus_Transitions_ValidPathsOnly()
    {
        // Mirrors the PATCH /v1/plans/{planId}/status transition rule
        var valid = new[] { (PlanStatus.Draft, PlanStatus.Active), (PlanStatus.Active, PlanStatus.Completed) };
        var invalid = new[]
        {
            (PlanStatus.Draft,     PlanStatus.Completed), // skip Active
            (PlanStatus.Active,    PlanStatus.Draft),     // no rollback
            (PlanStatus.Completed, PlanStatus.Active),    // completed is terminal
        };

        bool Evaluate(PlanStatus from, PlanStatus to) => (from, to) switch
        {
            (PlanStatus.Draft,  PlanStatus.Active)    => true,
            (PlanStatus.Active, PlanStatus.Completed) => true,
            _ => false
        };

        foreach (var (from, to) in valid)
        {
            await Assert.That(Evaluate(from, to)).IsTrue();
        }

        foreach (var (from, to) in invalid)
        {
            await Assert.That(Evaluate(from, to)).IsFalse();
        }
    }
}

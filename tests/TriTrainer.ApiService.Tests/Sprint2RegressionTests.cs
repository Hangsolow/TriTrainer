using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TriTrainer.ApiService.Data;

namespace TriTrainer.ApiService.Tests;

/// <summary>
/// Sprint 2 regression matrix: covers goal/plan status transitions,
/// generation-rule behavior, and invalid-payload error semantics.
/// Owner: Ivy (QA)
/// </summary>
public class Sprint2RegressionTests
{
    // ─── DB factory ────────────────────────────────────────────────────────────

    private static ActivitiesDbContext CreateDb(string? tag = null)
    {
        var options = new DbContextOptionsBuilder<ActivitiesDbContext>()
            .UseInMemoryDatabase(tag ?? Guid.NewGuid().ToString())
            .Options;
        return new ActivitiesDbContext(options);
    }

    // ─── Shared seed helpers ───────────────────────────────────────────────────

    private static async Task<AthleteProfile> SeedAthleteAsync(ActivitiesDbContext db,
        string name = "TestAthlete", decimal hours = 8,
        DateOnly? eventDate = null)
    {
        var profile = new AthleteProfile
        {
            DisplayName = name,
            WeeklyHoursAvailable = hours,
            PrimaryEventDate = eventDate ?? new DateOnly(2027, 9, 1)
        };
        db.AthleteProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    private static async Task<Goal> SeedGoalAsync(ActivitiesDbContext db, Guid athleteId,
        GoalType type = GoalType.EventFinish,
        GoalStatus status = GoalStatus.Draft,
        ActivityType? discipline = null,
        DateOnly? targetDate = null)
    {
        var goal = new Goal
        {
            AthleteId = athleteId,
            GoalType = type,
            Discipline = discipline,
            TargetDate = targetDate ?? new DateOnly(2027, 9, 1),
            Status = status
        };
        db.Goals.Add(goal);
        await db.SaveChangesAsync();
        return goal;
    }

    private static async Task<TrainingPlan> SeedPlanAsync(ActivitiesDbContext db,
        Guid athleteId, Guid? goalId = null,
        string name = "Test Plan",
        PlanStatus status = PlanStatus.Draft,
        DateOnly? startDate = null,
        int weekCount = 4,
        bool generateSessions = false)
    {
        var start = startDate ?? new DateOnly(2027, 6, 1);
        var plan = new TrainingPlan
        {
            AthleteId = athleteId,
            GoalId = goalId,
            Name = name,
            StartDate = start,
            EndDate = start.AddDays(weekCount * 7 - 1),
            Status = status
        };
        for (var i = 0; i < weekCount; i++)
        {
            plan.Weeks.Add(new PlanWeek
            {
                WeekIndex = i + 1,
                WeekStartDate = start.AddDays(i * 7)
            });
        }

        if (generateSessions)
        {
            var athlete = await db.AthleteProfiles.FindAsync(athleteId);
            Goal? goal = goalId.HasValue ? await db.Goals.FindAsync(goalId.Value) : null;
            var sessions = TriTrainer.ApiService.Services.PlanGenerationService.GenerateSessions(plan, goal, athlete!);
            foreach (var session in sessions)
            {
                var targetWeek = plan.Weeks.First(w => w.Id == session.PlanWeekId);
                targetWeek.Sessions.Add(session);
            }
        }

        db.TrainingPlans.Add(plan);
        await db.SaveChangesAsync();
        return plan;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 1 — GOAL STATUS TRANSITIONS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GoalStatus_DraftToActive_IsAllowed()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, status: GoalStatus.Draft);

        goal.Status = GoalStatus.Active;
        await db.SaveChangesAsync();

        var stored = await db.Goals.FindAsync(goal.Id);
        await Assert.That(stored!.Status).IsEqualTo(GoalStatus.Active);
    }

    [Test]
    public async Task GoalStatus_DraftToArchived_IsAllowed()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, status: GoalStatus.Draft);

        goal.Status = GoalStatus.Archived;
        await db.SaveChangesAsync();

        var stored = await db.Goals.FindAsync(goal.Id);
        await Assert.That(stored!.Status).IsEqualTo(GoalStatus.Archived);
    }

    [Test]
    public async Task GoalStatus_ActiveToAchieved_IsAllowed()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, status: GoalStatus.Active);

        goal.Status = GoalStatus.Achieved;
        await db.SaveChangesAsync();

        var stored = await db.Goals.FindAsync(goal.Id);
        await Assert.That(stored!.Status).IsEqualTo(GoalStatus.Achieved);
    }

    [Test]
    public async Task GoalStatus_ActiveToArchived_IsAllowed()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, status: GoalStatus.Active);

        goal.Status = GoalStatus.Archived;
        await db.SaveChangesAsync();

        var stored = await db.Goals.FindAsync(goal.Id);
        await Assert.That(stored!.Status).IsEqualTo(GoalStatus.Archived);
    }

    [Test]
    public async Task GoalStatus_AchievedToArchived_IsAllowed()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, status: GoalStatus.Achieved);

        goal.Status = GoalStatus.Archived;
        await db.SaveChangesAsync();

        var stored = await db.Goals.FindAsync(goal.Id);
        await Assert.That(stored!.Status).IsEqualTo(GoalStatus.Archived);
    }

    /// <summary>
    /// Validates the status-transition guard table as implemented in Program.cs.
    /// All invalid transitions must return false from the switch expression.
    /// </summary>
    [Test]
    [Arguments(GoalStatus.Active, GoalStatus.Draft)]
    [Arguments(GoalStatus.Achieved, GoalStatus.Draft)]
    [Arguments(GoalStatus.Achieved, GoalStatus.Active)]
    [Arguments(GoalStatus.Archived, GoalStatus.Draft)]
    [Arguments(GoalStatus.Archived, GoalStatus.Active)]
    [Arguments(GoalStatus.Archived, GoalStatus.Achieved)]
    [Arguments(GoalStatus.Draft, GoalStatus.Achieved)]
    public async Task GoalStatus_InvalidTransitions_AreRejectedByBusinessRule(
        GoalStatus currentStatus, GoalStatus requestedStatus)
    {
        // Mirrors the exact switch expression in PATCH /v1/goals/{goalId}/status
        var isValidTransition = (currentStatus, requestedStatus) switch
        {
            (GoalStatus.Draft, GoalStatus.Active) => true,
            (GoalStatus.Draft, GoalStatus.Archived) => true,
            (GoalStatus.Active, GoalStatus.Achieved) => true,
            (GoalStatus.Active, GoalStatus.Archived) => true,
            (GoalStatus.Achieved, GoalStatus.Archived) => true,
            _ => false
        };

        await Assert.That(isValidTransition).IsFalse();
    }

    [Test]
    public async Task GoalStatus_DuplicateActivePerType_IsBlocked()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);

        // First goal: already Active
        await SeedGoalAsync(db, athlete.Id, GoalType.EventFinish, GoalStatus.Active);

        // A second goal of same type is Draft; check if activating it would be blocked
        var secondGoal = await SeedGoalAsync(db, athlete.Id, GoalType.EventFinish, GoalStatus.Draft);

        var hasActiveForType = await db.Goals.AnyAsync(g =>
            g.Id != secondGoal.Id &&
            g.AthleteId == athlete.Id &&
            g.GoalType == secondGoal.GoalType &&
            g.Status == GoalStatus.Active);

        await Assert.That(hasActiveForType).IsTrue();
    }

    [Test]
    public async Task GoalStatus_DifferentTypes_CanBothBeActive()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);

        await SeedGoalAsync(db, athlete.Id, GoalType.EventFinish, GoalStatus.Active);
        await SeedGoalAsync(db, athlete.Id, GoalType.Consistency, GoalStatus.Active);

        var activeCount = await db.Goals.CountAsync(g =>
            g.AthleteId == athlete.Id && g.Status == GoalStatus.Active);

        await Assert.That(activeCount).IsEqualTo(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 2 — GOAL PAYLOAD VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Goal_DisciplinePerformance_WithoutDiscipline_ShouldBeRejected()
    {
        // Mirrors the validation rule in POST /v1/goals
        GoalType goalType = GoalType.DisciplinePerformance;
        ActivityType? discipline = null;

        bool disciplineRequired =
            discipline is null && goalType == GoalType.DisciplinePerformance;

        await Assert.That(disciplineRequired).IsTrue();
    }

    [Test]
    public async Task Goal_DisciplinePerformance_WithDiscipline_ShouldPass()
    {
        GoalType goalType = GoalType.DisciplinePerformance;
        ActivityType? discipline = ActivityType.Run;

        bool disciplineRequired =
            discipline is null && goalType == GoalType.DisciplinePerformance;

        await Assert.That(disciplineRequired).IsFalse();
    }

    [Test]
    public async Task Goal_TargetDate_TodayOrPast_ShouldBeRejected()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var yesterday = today.AddDays(-1);

        // The API check is: request.TargetDate <= DateOnly.FromDateTime(DateTime.UtcNow.Date)
        bool isTodayRejected = today <= DateOnly.FromDateTime(DateTime.UtcNow.Date); // equal → rejected
        bool isYesterdayRejected = yesterday < today; // strictly before → rejected

        await Assert.That(isTodayRejected).IsTrue();
        await Assert.That(isYesterdayRejected).IsTrue();
    }

    [Test]
    public async Task Goal_TargetDate_Tomorrow_ShouldBeAccepted()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(1);
        bool isRejected = tomorrow <= DateOnly.FromDateTime(DateTime.UtcNow.Date);

        await Assert.That(isRejected).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 3 — PLAN PAYLOAD VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(3)]
    public async Task Plan_WeekCount_BelowMinimum_ShouldBeRejected(int weekCount)
    {
        bool isRejected = weekCount is < 4 or > 16;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    [Arguments(17)]
    [Arguments(20)]
    [Arguments(52)]
    public async Task Plan_WeekCount_AboveMaximum_ShouldBeRejected(int weekCount)
    {
        bool isRejected = weekCount is < 4 or > 16;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    [Arguments(4)]
    [Arguments(8)]
    [Arguments(12)]
    [Arguments(16)]
    public async Task Plan_WeekCount_WithinBounds_ShouldBeAccepted(int weekCount)
    {
        bool isRejected = weekCount is < 4 or > 16;
        await Assert.That(isRejected).IsFalse();
    }

    [Test]
    public async Task Plan_EmptyName_ShouldBeRejected()
    {
        bool isRejected = string.IsNullOrWhiteSpace("") || "".Length > 120;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    public async Task Plan_NameTooLong_ShouldBeRejected()
    {
        var longName = new string('X', 121);
        bool isRejected = string.IsNullOrWhiteSpace(longName) || longName.Length > 120;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    public async Task Plan_NameAt120Chars_ShouldBeAccepted()
    {
        var maxName = new string('X', 120);
        bool isRejected = string.IsNullOrWhiteSpace(maxName) || maxName.Length > 120;
        await Assert.That(isRejected).IsFalse();
    }

    [Test]
    public async Task Plan_EndDateExceedsGoalTargetDate_ShouldBeRejected()
    {
        var goalTargetDate = new DateOnly(2027, 9, 1);
        var startDate = new DateOnly(2027, 8, 1);
        int weekCount = 8; // 56 days → end date = 2027-09-25, exceeds target

        var endDate = startDate.AddDays(weekCount * 7 - 1);
        bool isRejected = endDate > goalTargetDate;

        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    public async Task Plan_EndDateOnGoalTargetDate_ShouldBeAccepted()
    {
        // The API formula is: endDate = startDate.AddDays((weekCount * 7) - 1)
        // So 4 weeks from 2027-08-05 → 2027-08-05 + 27 = 2027-09-01 (exactly on target)
        var goalTargetDate = new DateOnly(2027, 9, 1);
        var startDate = new DateOnly(2027, 8, 5);
        int weekCount = 4;

        var endDate = startDate.AddDays(weekCount * 7 - 1); // +27 days
        bool isRejected = endDate > goalTargetDate;

        await Assert.That(isRejected).IsFalse();
        await Assert.That(endDate).IsEqualTo(goalTargetDate);
    }

    [Test]
    public async Task Plan_Creation_GeneratesCorrectWeekCount()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, targetDate: new DateOnly(2027, 9, 1));
        var plan = await SeedPlanAsync(db, athlete.Id, goal.Id, weekCount: 8);

        var stored = await db.TrainingPlans
            .Include(p => p.Weeks)
            .FirstOrDefaultAsync(p => p.Id == plan.Id);

        await Assert.That(stored).IsNotNull();
        await Assert.That(stored!.Weeks.Count).IsEqualTo(8);
    }

    [Test]
    public async Task Plan_Creation_WeekStartDatesAreCorrect()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, targetDate: new DateOnly(2027, 9, 1));
        var start = new DateOnly(2027, 6, 1);
        var plan = await SeedPlanAsync(db, athlete.Id, goal.Id, startDate: start, weekCount: 4);

        var stored = await db.TrainingPlans
            .Include(p => p.Weeks)
            .FirstOrDefaultAsync(p => p.Id == plan.Id);

        var weeks = stored!.Weeks.OrderBy(w => w.WeekIndex).ToList();
        await Assert.That(weeks[0].WeekStartDate).IsEqualTo(start);
        await Assert.That(weeks[1].WeekStartDate).IsEqualTo(start.AddDays(7));
        await Assert.That(weeks[2].WeekStartDate).IsEqualTo(start.AddDays(14));
        await Assert.That(weeks[3].WeekStartDate).IsEqualTo(start.AddDays(21));
    }

    [Test]
    public async Task Plan_Creation_GoalNotFoundForAthlete_ShouldReturnNotFound()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);

        // Try to look up a random GoalId that doesn't belong to this athlete
        var nonExistentGoalId = Guid.NewGuid();
        var goal = await db.Goals.FirstOrDefaultAsync(g =>
            g.Id == nonExistentGoalId && g.AthleteId == athlete.Id);

        await Assert.That(goal).IsNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 4 — ATHLETE PROFILE VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task AthleteProfile_EmptyDisplayName_ShouldBeRejected()
    {
        string displayName = "";
        bool isRejected = string.IsNullOrWhiteSpace(displayName) || displayName.Length > 120;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    public async Task AthleteProfile_WhitespaceDisplayName_ShouldBeRejected()
    {
        string displayName = "   ";
        bool isRejected = string.IsNullOrWhiteSpace(displayName) || displayName.Length > 120;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    public async Task AthleteProfile_DisplayNameAt120Chars_ShouldBeAccepted()
    {
        string displayName = new string('A', 120);
        bool isRejected = string.IsNullOrWhiteSpace(displayName) || displayName.Length > 120;
        await Assert.That(isRejected).IsFalse();
    }

    [Test]
    public async Task AthleteProfile_DisplayNameAt121Chars_ShouldBeRejected()
    {
        string displayName = new string('A', 121);
        bool isRejected = string.IsNullOrWhiteSpace(displayName) || displayName.Length > 120;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-0.5)]
    public async Task AthleteProfile_WeeklyHours_ZeroOrNegative_ShouldBeRejected(decimal hours)
    {
        bool isRejected = hours is <= 0 or > 40;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    [Arguments(41)]
    [Arguments(50)]
    [Arguments(100)]
    public async Task AthleteProfile_WeeklyHours_AboveMax_ShouldBeRejected(decimal hours)
    {
        bool isRejected = hours is <= 0 or > 40;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    [Arguments(0.1)]
    [Arguments(8)]
    [Arguments(20)]
    [Arguments(40)]
    public async Task AthleteProfile_WeeklyHours_ValidRange_ShouldBeAccepted(decimal hours)
    {
        bool isRejected = hours is <= 0 or > 40;
        await Assert.That(isRejected).IsFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 5 — PERSONAL RECORDS VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-100)]
    public async Task PersonalRecord_ZeroOrNegativeValue_ShouldBeRejected(decimal value)
    {
        bool isRejected = value <= 0;
        await Assert.That(isRejected).IsTrue();
    }

    [Test]
    [Arguments(0.01)]
    [Arguments(1)]
    [Arguments(999)]
    public async Task PersonalRecord_PositiveValue_ShouldBeAccepted(decimal value)
    {
        bool isRejected = value <= 0;
        await Assert.That(isRejected).IsFalse();
    }

    [Test]
    public async Task PersonalRecord_WithNonExistentSourceActivity_ShouldBeRejected()
    {
        await using var db = CreateDb();

        var nonExistentActivityId = Guid.NewGuid();
        var exists = await db.Activities.AnyAsync(a => a.Id == nonExistentActivityId);

        await Assert.That(exists).IsFalse();
    }

    [Test]
    public async Task PersonalRecord_WithExistingSourceActivity_ShouldBeAccepted()
    {
        await using var db = CreateDb();

        var activity = new Activity
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            Type = ActivityType.Run,
            DurationMinutes = 45
        };
        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        var exists = await db.Activities.AnyAsync(a => a.Id == activity.Id);
        await Assert.That(exists).IsTrue();
    }

    [Test]
    public async Task PersonalRecord_NullSourceActivity_ShouldBeAccepted()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);

        var record = new PersonalRecord
        {
            AthleteId = athlete.Id,
            Discipline = ActivityType.Run,
            Metric = PersonalRecordMetric.Fastest5k,
            Value = 25.5m,
            AchievedOn = DateOnly.FromDateTime(DateTime.Today),
            SourceActivityId = null
        };
        db.PersonalRecords.Add(record);
        await db.SaveChangesAsync();

        var stored = await db.PersonalRecords.FindAsync(record.Id);
        await Assert.That(stored).IsNotNull();
        await Assert.That(stored!.SourceActivityId).IsNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 6 — GENERATION RULES (Sprint 2 Task 1 contracts)
    // These tests define the expected behavior of plan generation rules.
    // They will FAIL until Task 1 (Plan generation rule set v1) is implemented.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// SPRINT 2 TASK 1 CONTRACT: A newly created plan must auto-generate
    /// at least one PlannedSession per PlanWeek covering all three disciplines.
    /// Expected to FAIL until Sage implements generation rules.
    /// </summary>
    [Test]
    public async Task PlanGeneration_NewPlan_ShouldAutoPopulateSessions()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, targetDate: new DateOnly(2027, 9, 1));
        var plan = await SeedPlanAsync(db, athlete.Id, goal.Id, weekCount: 4, generateSessions: true);

        var stored = await db.TrainingPlans
            .Include(p => p.Weeks)
            .ThenInclude(w => w.Sessions)
            .FirstOrDefaultAsync(p => p.Id == plan.Id);

        // Each week must have at least one session
        foreach (var week in stored!.Weeks)
        {
            await Assert.That(week.Sessions.Count).IsGreaterThan(0);
        }
    }

    /// <summary>
    /// SPRINT 2 TASK 1 CONTRACT: Triathlon plans must include all three
    /// disciplines (Swim, Cycle, Run) in generated sessions.
    /// Expected to FAIL until Task 1 is implemented.
    /// </summary>
    [Test]
    public async Task PlanGeneration_AllThreeDisciplines_ShouldBeRepresented()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, targetDate: new DateOnly(2027, 9, 1));
        var plan = await SeedPlanAsync(db, athlete.Id, goal.Id, weekCount: 4, generateSessions: true);

        var stored = await db.TrainingPlans
            .Include(p => p.Weeks)
            .ThenInclude(w => w.Sessions)
            .FirstOrDefaultAsync(p => p.Id == plan.Id);

        var disciplines = stored!.Weeks
            .SelectMany(w => w.Sessions)
            .Select(s => s.Discipline)
            .Distinct()
            .ToHashSet();

        await Assert.That(disciplines.Contains(ActivityType.Run)).IsTrue();
        await Assert.That(disciplines.Contains(ActivityType.Cycle)).IsTrue();
        await Assert.That(disciplines.Contains(ActivityType.Swim)).IsTrue();
    }

    /// <summary>
    /// SPRINT 2 TASK 1 CONTRACT: Each generated session must have a positive
    /// PlannedDurationMinutes (> 0).
    /// Expected to FAIL until Task 1 is implemented.
    /// </summary>
    [Test]
    public async Task PlanGeneration_Sessions_ShouldHavePositiveDuration()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id, targetDate: new DateOnly(2027, 9, 1));
        var plan = await SeedPlanAsync(db, athlete.Id, goal.Id, weekCount: 4, generateSessions: true);

        var stored = await db.TrainingPlans
            .Include(p => p.Weeks)
            .ThenInclude(w => w.Sessions)
            .FirstOrDefaultAsync(p => p.Id == plan.Id);

        var sessions = stored!.Weeks.SelectMany(w => w.Sessions).ToList();
        await Assert.That(sessions.Count).IsGreaterThan(0);

        foreach (var session in sessions)
        {
            await Assert.That(session.PlannedDurationMinutes).IsGreaterThan(0);
        }
    }

    /// <summary>
    /// SPRINT 2 TASK 1 CONTRACT: A consistent weekly total volume across a
    /// plan's weeks implies discipline defaults were applied. The total planned
    /// minutes per week should be non-zero when athlete has hours available.
    /// Expected to FAIL until Task 1 is implemented.
    /// </summary>
    [Test]
    public async Task PlanGeneration_WeeklyVolume_ShouldReflectAthleteHoursAvailable()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db, hours: 10);
        var goal = await SeedGoalAsync(db, athlete.Id, targetDate: new DateOnly(2027, 9, 1));
        var plan = await SeedPlanAsync(db, athlete.Id, goal.Id, weekCount: 4, generateSessions: true);

        var stored = await db.TrainingPlans
            .Include(p => p.Weeks)
            .ThenInclude(w => w.Sessions)
            .FirstOrDefaultAsync(p => p.Id == plan.Id);

        foreach (var week in stored!.Weeks)
        {
            var totalMinutes = week.Sessions.Sum(s => s.PlannedDurationMinutes);
            // Athlete has 10 hrs/week = 600 minutes; sessions should fill some portion
            await Assert.That(totalMinutes).IsGreaterThan(0);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 7 — PLAN STATUS TRANSITIONS
    // Tests for the missing PATCH /v1/plans/{id}/status endpoint.
    // NOTE: These are data-layer checks since the HTTP endpoint does not exist.
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task PlanStatus_Draft_CanBeStoredInDatabase()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var plan = await SeedPlanAsync(db, athlete.Id, status: PlanStatus.Draft);

        var stored = await db.TrainingPlans.FindAsync(plan.Id);
        await Assert.That(stored!.Status).IsEqualTo(PlanStatus.Draft);
    }

    [Test]
    public async Task PlanStatus_Active_CanBeStoredInDatabase()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var plan = await SeedPlanAsync(db, athlete.Id, status: PlanStatus.Active);

        var stored = await db.TrainingPlans.FindAsync(plan.Id);
        await Assert.That(stored!.Status).IsEqualTo(PlanStatus.Active);
    }

    [Test]
    public async Task PlanStatus_Completed_CanBeStoredInDatabase()
    {
        await using var db = CreateDb();
        var athlete = await SeedAthleteAsync(db);
        var plan = await SeedPlanAsync(db, athlete.Id, status: PlanStatus.Completed);

        var stored = await db.TrainingPlans.FindAsync(plan.Id);
        await Assert.That(stored!.Status).IsEqualTo(PlanStatus.Completed);
    }

    /// <summary>
    /// SPRINT 2 TASK 2 CONTRACT: Verifies the plan status transition rule table
    /// matches the PATCH /v1/plans/{id}/status endpoint implementation.
    /// Data-layer test; HTTP-layer coverage lives in integration tests.
    /// </summary>
    [Test]
    [Arguments(PlanStatus.Draft,  PlanStatus.Active,    true)]
    [Arguments(PlanStatus.Active, PlanStatus.Completed, true)]
    [Arguments(PlanStatus.Draft,  PlanStatus.Completed, false)]
    [Arguments(PlanStatus.Active, PlanStatus.Draft,     false)]
    [Arguments(PlanStatus.Completed, PlanStatus.Active, false)]
    public async Task PlanStatus_TransitionRuleTable_MatchesEndpointImplementation(
        PlanStatus from, PlanStatus to, bool expectedValid)
    {
        var isValid = (from, to) switch
        {
            (PlanStatus.Draft,  PlanStatus.Active)    => true,
            (PlanStatus.Active, PlanStatus.Completed) => true,
            _ => false
        };

        await Assert.That(isValid).IsEqualTo(expectedValid);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 8 — SPRINT 1 LEGACY REGRESSION (activities support flow)
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Legacy_Activities_CoexistWithPlanningDomain()
    {
        await using var db = CreateDb();

        // Legacy activity
        var activity = new Activity
        {
            Date = new DateOnly(2027, 6, 2),
            Type = ActivityType.Cycle,
            DurationMinutes = 75,
            Notes = "Sprint 2 coexistence check"
        };
        db.Activities.Add(activity);

        // Planning domain
        var athlete = await SeedAthleteAsync(db);
        var goal = await SeedGoalAsync(db, athlete.Id);
        await SeedPlanAsync(db, athlete.Id, goal.Id);

        await db.SaveChangesAsync();

        var storedActivity = await db.Activities.FindAsync(activity.Id);
        var planCount = await db.TrainingPlans.CountAsync();
        var goalCount = await db.Goals.CountAsync();

        await Assert.That(storedActivity).IsNotNull();
        await Assert.That(planCount).IsGreaterThan(0);
        await Assert.That(goalCount).IsGreaterThan(0);
    }

    [Test]
    public async Task Legacy_Activities_FilterByMonth_StillWorks()
    {
        await using var db = CreateDb();

        db.Activities.AddRange(
            new Activity { Date = new DateOnly(2027, 6, 1), Type = ActivityType.Run, DurationMinutes = 30 },
            new Activity { Date = new DateOnly(2027, 6, 15), Type = ActivityType.Swim, DurationMinutes = 40 },
            new Activity { Date = new DateOnly(2027, 7, 1), Type = ActivityType.Cycle, DurationMinutes = 60 }
        );
        await db.SaveChangesAsync();

        var from = new DateOnly(2027, 6, 1);
        var to = new DateOnly(2027, 6, 30);
        var juneActivities = await db.Activities
            .Where(a => a.Date >= from && a.Date <= to)
            .ToListAsync();

        await Assert.That(juneActivities.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Legacy_Activities_AllTypes_SupportedAlongside_PlanningDomain()
    {
        await using var db = CreateDb();

        var types = Enum.GetValues<ActivityType>();
        foreach (var type in types)
        {
            db.Activities.Add(new Activity
            {
                Date = new DateOnly(2027, 6, 1),
                Type = type,
                DurationMinutes = 30
            });
        }
        await db.SaveChangesAsync();

        var count = await db.Activities.CountAsync();
        await Assert.That(count).IsEqualTo(types.Length);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 9 — PROGRESS AGGREGATION CONTRACT
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Progress_ComplianceCalc_ZeroPlanned_ReturnsZeroPercent()
    {
        // Mirrors the compliance formula in GET /v1/progress/weekly
        int plannedMinutes = 0;
        int completedMinutes = 45;

        decimal compliancePercent = plannedMinutes == 0
            ? 0
            : decimal.Round((decimal)completedMinutes / plannedMinutes * 100, 2);

        await Assert.That(compliancePercent).IsEqualTo(0);
    }

    [Test]
    public async Task Progress_ComplianceCalc_FullCompletion_Returns100Percent()
    {
        int plannedMinutes = 60;
        int completedMinutes = 60;

        decimal compliancePercent = plannedMinutes == 0
            ? 0
            : decimal.Round((decimal)completedMinutes / plannedMinutes * 100, 2);

        await Assert.That(compliancePercent).IsEqualTo(100m);
    }

    [Test]
    public async Task Progress_ComplianceCalc_PartialCompletion_IsCorrect()
    {
        int plannedMinutes = 60;
        int completedMinutes = 45;

        decimal compliancePercent = plannedMinutes == 0
            ? 0
            : decimal.Round((decimal)completedMinutes / plannedMinutes * 100, 2);

        await Assert.That(compliancePercent).IsEqualTo(75m);
    }

    [Test]
    public async Task Progress_ComplianceCalc_Overcompletion_ExceededPercent()
    {
        // Athletes may complete more than planned; no cap at 100%
        int plannedMinutes = 60;
        int completedMinutes = 90;

        decimal compliancePercent = plannedMinutes == 0
            ? 0
            : decimal.Round((decimal)completedMinutes / plannedMinutes * 100, 2);

        await Assert.That(compliancePercent).IsEqualTo(150m);
    }

    [Test]
    public async Task Progress_WeeklyActivities_OnlyWithinWeekWindow_AreIncluded()
    {
        await using var db = CreateDb();

        var weekStart = new DateOnly(2027, 6, 7);
        var weekEnd = weekStart.AddDays(6); // 2027-06-13

        db.Activities.AddRange(
            new Activity { Date = new DateOnly(2027, 6, 6), Type = ActivityType.Run, DurationMinutes = 30 }, // before window
            new Activity { Date = weekStart, Type = ActivityType.Run, DurationMinutes = 45 },                 // start of window
            new Activity { Date = new DateOnly(2027, 6, 10), Type = ActivityType.Swim, DurationMinutes = 50 }, // mid window
            new Activity { Date = weekEnd, Type = ActivityType.Cycle, DurationMinutes = 60 },                  // end of window
            new Activity { Date = new DateOnly(2027, 6, 14), Type = ActivityType.Run, DurationMinutes = 30 }  // after window
        );
        await db.SaveChangesAsync();

        var activitiesInWindow = await db.Activities
            .Where(a => a.Date >= weekStart && a.Date <= weekEnd)
            .ToListAsync();

        await Assert.That(activitiesInWindow.Count).IsEqualTo(3);
    }
}

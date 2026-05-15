using Microsoft.EntityFrameworkCore;
using TriTrainer.ApiService.Data;

namespace TriTrainer.ApiService.Tests;

public class PlanningDomainTests
{
    private static ActivitiesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ActivitiesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ActivitiesDbContext(options);
    }

    [Test]
    public async Task PlanningEntities_CanBeCreated_AlongsideExistingActivities()
    {
        await using var db = CreateDb();

        var athlete = new AthleteProfile
        {
            DisplayName = "Alex",
            WeeklyHoursAvailable = 8,
            PrimaryEventDate = new DateOnly(2026, 10, 4)
        };

        var goal = new Goal
        {
            AthleteId = athlete.Id,
            GoalType = GoalType.EventFinish,
            TargetDate = new DateOnly(2026, 10, 4),
            Status = GoalStatus.Active
        };

        var plan = new TrainingPlan
        {
            AthleteId = athlete.Id,
            GoalId = goal.Id,
            Name = "Build Block",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 6, 28),
            Status = PlanStatus.Draft
        };

        var week = new PlanWeek
        {
            PlanId = plan.Id,
            WeekIndex = 1,
            WeekStartDate = new DateOnly(2026, 6, 1)
        };

        var session = new PlannedSession
        {
            PlanWeekId = week.Id,
            Discipline = ActivityType.Run,
            SessionType = SessionType.Endurance,
            PlannedDurationMinutes = 60,
            DayOfWeek = DayOfWeek.Monday
        };

        var activity = new Activity
        {
            Date = new DateOnly(2026, 6, 1),
            Type = ActivityType.Run,
            DurationMinutes = 55,
            Notes = "Steady run"
        };

        db.AthleteProfiles.Add(athlete);
        db.Goals.Add(goal);
        db.TrainingPlans.Add(plan);
        db.PlanWeeks.Add(week);
        db.PlannedSessions.Add(session);
        db.Activities.Add(activity);

        await db.SaveChangesAsync();

        var storedActivity = await db.Activities.FindAsync(activity.Id);
        var storedPlan = await db.TrainingPlans.Include(p => p.Weeks).FirstOrDefaultAsync(p => p.Id == plan.Id);

        await Assert.That(storedActivity).IsNotNull();
        await Assert.That(storedPlan).IsNotNull();
        await Assert.That(storedPlan!.Weeks.Count).IsEqualTo(1);
    }

    [Test]
    public async Task WeeklyProgressAggregation_UsesPlannedAndCompletedMinutesByDiscipline()
    {
        await using var db = CreateDb();

        var athlete = new AthleteProfile
        {
            DisplayName = "Jordan",
            WeeklyHoursAvailable = 10
        };
        db.AthleteProfiles.Add(athlete);

        var goal = new Goal
        {
            AthleteId = athlete.Id,
            GoalType = GoalType.Consistency,
            TargetDate = new DateOnly(2026, 7, 1)
        };
        db.Goals.Add(goal);

        var plan = new TrainingPlan
        {
            AthleteId = athlete.Id,
            GoalId = goal.Id,
            Name = "Week Focus",
            StartDate = new DateOnly(2026, 5, 4),
            EndDate = new DateOnly(2026, 5, 31)
        };
        db.TrainingPlans.Add(plan);

        var week = new PlanWeek
        {
            PlanId = plan.Id,
            WeekIndex = 1,
            WeekStartDate = new DateOnly(2026, 5, 4)
        };
        db.PlanWeeks.Add(week);

        db.PlannedSessions.AddRange(
            new PlannedSession
            {
                PlanWeekId = week.Id,
                Discipline = ActivityType.Run,
                SessionType = SessionType.Tempo,
                PlannedDurationMinutes = 50,
                DayOfWeek = DayOfWeek.Tuesday
            },
            new PlannedSession
            {
                PlanWeekId = week.Id,
                Discipline = ActivityType.Swim,
                SessionType = SessionType.Endurance,
                PlannedDurationMinutes = 40,
                DayOfWeek = DayOfWeek.Thursday
            });

        db.Activities.AddRange(
            new Activity
            {
                Date = new DateOnly(2026, 5, 5),
                Type = ActivityType.Run,
                DurationMinutes = 45
            },
            new Activity
            {
                Date = new DateOnly(2026, 5, 8),
                Type = ActivityType.Swim,
                DurationMinutes = 35
            },
            new Activity
            {
                Date = new DateOnly(2026, 5, 20),
                Type = ActivityType.Cycle,
                DurationMinutes = 80
            });

        await db.SaveChangesAsync();

        var weekStart = new DateOnly(2026, 5, 4);
        var weekEnd = weekStart.AddDays(6);
        var completedWeekActivities = await db.Activities
            .Where(a => a.Date >= weekStart && a.Date <= weekEnd)
            .ToListAsync();

        var runPlanned = await db.PlannedSessions
            .Where(s => s.PlanWeekId == week.Id && s.Discipline == ActivityType.Run)
            .SumAsync(s => s.PlannedDurationMinutes);
        var runCompleted = completedWeekActivities
            .Where(a => a.Type == ActivityType.Run)
            .Sum(a => a.DurationMinutes);

        var swimPlanned = await db.PlannedSessions
            .Where(s => s.PlanWeekId == week.Id && s.Discipline == ActivityType.Swim)
            .SumAsync(s => s.PlannedDurationMinutes);
        var swimCompleted = completedWeekActivities
            .Where(a => a.Type == ActivityType.Swim)
            .Sum(a => a.DurationMinutes);

        await Assert.That(runPlanned).IsEqualTo(50);
        await Assert.That(runCompleted).IsEqualTo(45);
        await Assert.That(swimPlanned).IsEqualTo(40);
        await Assert.That(swimCompleted).IsEqualTo(35);
    }
}

using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TriTrainer.ApiService.Data;
using TriTrainer.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.AddNpgsqlDbContext<ActivitiesDbContext>("tritrainerdb");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ActivitiesDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/activities", async (int? year, int? month, ActivitiesDbContext db) =>
{
    var query = db.Activities.AsQueryable();

    if (year.HasValue && month.HasValue)
    {
        var from = new DateOnly(year.Value, month.Value, 1);
        var to = from.AddMonths(1).AddDays(-1);
        query = query.Where(a => a.Date >= from && a.Date <= to);
    }

    return await query.OrderBy(a => a.Date).ThenBy(a => a.CreatedAt).ToListAsync();
})
.WithName("GetActivities");

app.MapPost("/activities", async (CreateActivityRequest request, ActivitiesDbContext db) =>
{
    var activity = new Activity
    {
        Date = request.Date,
        Type = request.Type,
        DurationMinutes = request.DurationMinutes,
        Notes = request.Notes
    };

    db.Activities.Add(activity);
    await db.SaveChangesAsync();

    return Results.Created($"/activities/{activity.Id}", activity);
})
.WithName("CreateActivity");

app.MapDelete("/activities/{id:guid}", async (Guid id, ActivitiesDbContext db) =>
{
    var activity = await db.Activities.FindAsync(id);
    if (activity is null) return Results.NotFound();

    db.Activities.Remove(activity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteActivity");

var v1 = app.MapGroup("/v1");

v1.MapGet("/athlete-profile", async (ActivitiesDbContext db) =>
{
    var profile = await db.AthleteProfiles.AsNoTracking().FirstOrDefaultAsync();
    return profile is null
        ? Results.NotFound()
        : Results.Ok(new AthleteProfileResponse(
            profile.Id,
            profile.DisplayName,
            profile.WeeklyHoursAvailable,
            profile.PrimaryEventDate,
            profile.CreatedAtUtc));
})
.WithName("GetAthleteProfile");

v1.MapPut("/athlete-profile", async (UpsertAthleteProfileRequest request, ActivitiesDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Length > 120)
    {
        return Results.BadRequest("DisplayName is required and must be at most 120 characters.");
    }

    if (request.WeeklyHoursAvailable is <= 0 or > 40)
    {
        return Results.BadRequest("WeeklyHoursAvailable must be between 0.1 and 40.");
    }

    var profile = await db.AthleteProfiles.FirstOrDefaultAsync();

    if (profile is null)
    {
        profile = new AthleteProfile
        {
            DisplayName = request.DisplayName.Trim(),
            WeeklyHoursAvailable = request.WeeklyHoursAvailable,
            PrimaryEventDate = request.PrimaryEventDate
        };

        db.AthleteProfiles.Add(profile);
    }
    else
    {
        profile.DisplayName = request.DisplayName.Trim();
        profile.WeeklyHoursAvailable = request.WeeklyHoursAvailable;
        profile.PrimaryEventDate = request.PrimaryEventDate;
    }

    await db.SaveChangesAsync();
    return Results.Ok(new AthleteProfileResponse(
        profile.Id,
        profile.DisplayName,
        profile.WeeklyHoursAvailable,
        profile.PrimaryEventDate,
        profile.CreatedAtUtc));
})
.WithName("UpsertAthleteProfile");

v1.MapGet("/goals", async (GoalStatus? status, ActivitiesDbContext db) =>
{
    var athlete = await db.AthleteProfiles.AsNoTracking().FirstOrDefaultAsync();
    if (athlete is null)
    {
        return Results.BadRequest("Athlete profile is required before creating goals.");
    }

    var query = db.Goals.AsNoTracking().Where(g => g.AthleteId == athlete.Id);
    if (status.HasValue)
    {
        query = query.Where(g => g.Status == status.Value);
    }

    var goals = await query
        .OrderBy(g => g.TargetDate)
        .ThenBy(g => g.CreatedAtUtc)
        .ToListAsync();

    return Results.Ok(goals.Select(g => new GoalResponse(
        g.Id,
        g.AthleteId,
        g.GoalType,
        g.Discipline,
        g.TargetValue,
        g.TargetDate,
        g.Status,
        g.CreatedAtUtc)));
})
.WithName("GetGoals");

v1.MapPost("/goals", async (CreateGoalRequest request, ActivitiesDbContext db) =>
{
    var athlete = await db.AthleteProfiles.FirstOrDefaultAsync();
    if (athlete is null)
    {
        return Results.BadRequest("Athlete profile is required before creating goals.");
    }

    if (request.TargetDate <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
    {
        return Results.BadRequest("TargetDate must be in the future.");
    }

    if (request.Discipline is null && request.GoalType == GoalType.DisciplinePerformance)
    {
        return Results.BadRequest("Discipline is required for DisciplinePerformance goals.");
    }

    if (request.GoalType == GoalType.DisciplinePerformance && (request.TargetValue is null or <= 0))
    {
        return Results.BadRequest("TargetValue is required and must be greater than 0 for DisciplinePerformance goals.");
    }

    var conflictingGoal = await db.Goals.AnyAsync(g =>
        g.AthleteId == athlete.Id &&
        g.GoalType == request.GoalType &&
        g.Status == GoalStatus.Active);

    if (conflictingGoal)
    {
        return Results.Conflict("Only one active goal per goal type is allowed.");
    }

    var goal = new Goal
    {
        AthleteId = athlete.Id,
        GoalType = request.GoalType,
        Discipline = request.Discipline,
        TargetValue = request.TargetValue,
        TargetDate = request.TargetDate,
        Status = GoalStatus.Draft
    };

    db.Goals.Add(goal);
    await db.SaveChangesAsync();
    return Results.Created($"/v1/goals/{goal.Id}", new GoalResponse(
        goal.Id,
        goal.AthleteId,
        goal.GoalType,
        goal.Discipline,
        goal.TargetValue,
        goal.TargetDate,
        goal.Status,
        goal.CreatedAtUtc));
})
.WithName("CreateGoal");

v1.MapPatch("/goals/{goalId:guid}/status", async (Guid goalId, UpdateGoalStatusRequest request, ActivitiesDbContext db) =>
{
    var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == goalId);
    if (goal is null)
    {
        return Results.NotFound();
    }

    var isValidTransition = (goal.Status, request.Status) switch
    {
        (GoalStatus.Draft, GoalStatus.Active) => true,
        (GoalStatus.Draft, GoalStatus.Archived) => true,
        (GoalStatus.Active, GoalStatus.Achieved) => true,
        (GoalStatus.Active, GoalStatus.Archived) => true,
        (GoalStatus.Achieved, GoalStatus.Archived) => true,
        _ => false
    };

    if (!isValidTransition)
    {
        return Results.Conflict("Invalid goal status transition.");
    }

    if (request.Status == GoalStatus.Active)
    {
        var hasActiveForType = await db.Goals.AnyAsync(g =>
            g.Id != goal.Id &&
            g.AthleteId == goal.AthleteId &&
            g.GoalType == goal.GoalType &&
            g.Status == GoalStatus.Active);

        if (hasActiveForType)
        {
            return Results.Conflict("Another active goal exists for this goal type.");
        }
    }

    goal.Status = request.Status;
    await db.SaveChangesAsync();
    return Results.Ok(new GoalResponse(
        goal.Id,
        goal.AthleteId,
        goal.GoalType,
        goal.Discipline,
        goal.TargetValue,
        goal.TargetDate,
        goal.Status,
        goal.CreatedAtUtc));
})
.WithName("UpdateGoalStatus");

v1.MapPost("/plans", async (CreatePlanRequest request, ActivitiesDbContext db) =>
{
    var athlete = await db.AthleteProfiles.FirstOrDefaultAsync();
    if (athlete is null)
    {
        return Results.BadRequest("Athlete profile is required before creating plans.");
    }

    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 120)
    {
        return Results.BadRequest("Name is required and must be at most 120 characters.");
    }

    if (request.WeekCount is < 4 or > 16)
    {
        return Results.BadRequest("WeekCount must be between 4 and 16.");
    }

    var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == request.GoalId && g.AthleteId == athlete.Id);
    if (goal is null)
    {
        return Results.NotFound("Goal not found.");
    }

    if (goal.Status is GoalStatus.Achieved or GoalStatus.Archived)
    {
        return Results.Conflict("Cannot create a plan for a goal that is already achieved or archived.");
    }

    var endDate = request.StartDate.AddDays((request.WeekCount * 7) - 1);
    if (endDate > goal.TargetDate)
    {
        return Results.BadRequest("Plan end date must be on or before goal target date.");
    }

    var plan = new TrainingPlan
    {
        AthleteId = athlete.Id,
        GoalId = goal.Id,
        Name = request.Name.Trim(),
        StartDate = request.StartDate,
        EndDate = endDate,
        Status = PlanStatus.Draft
    };

    for (var i = 0; i < request.WeekCount; i++)
    {
        plan.Weeks.Add(new PlanWeek
        {
            WeekIndex = i + 1,
            WeekStartDate = request.StartDate.AddDays(i * 7)
        });
    }

    // Generate discipline-specific sessions for every week using the athlete's available hours
    var generatedSessions = PlanGenerationService.GenerateSessions(plan, goal, athlete);
    foreach (var session in generatedSessions)
    {
        var targetWeek = plan.Weeks.First(w => w.Id == session.PlanWeekId);
        targetWeek.Sessions.Add(session);
    }

    db.TrainingPlans.Add(plan);
    await db.SaveChangesAsync();

    return Results.Created($"/v1/plans/{plan.Id}", new
    {
        plan.Id,
        plan.GoalId,
        plan.Name,
        plan.StartDate,
        plan.EndDate,
        plan.Status,
        WeekCount = plan.Weeks.Count,
        SessionCount = plan.Weeks.Sum(w => w.Sessions.Count)
    });
})
.WithName("CreatePlan");

v1.MapGet("/plans/{planId:guid}", async (Guid planId, ActivitiesDbContext db) =>
{
    var plan = await db.TrainingPlans
        .AsNoTracking()
        .Include(p => p.Weeks)
        .ThenInclude(w => w.Sessions)
        .FirstOrDefaultAsync(p => p.Id == planId);

    return plan is null
        ? Results.NotFound()
        : Results.Ok(new PlanDetailResponse(
            plan.Id,
            plan.AthleteId,
            plan.GoalId,
            plan.Name,
            plan.StartDate,
            plan.EndDate,
            plan.Status,
            plan.CreatedAtUtc,
            plan.Weeks
                .OrderBy(w => w.WeekIndex)
                .Select(w => new PlanWeekResponse(
                    w.Id,
                    w.PlanId,
                    w.WeekIndex,
                    w.WeekStartDate,
                    w.Notes,
                    w.Sessions
                        .OrderBy(s => s.DayOfWeek)
                        .Select(s => new PlannedSessionResponse(
                            s.Id,
                            s.PlanWeekId,
                            s.Discipline,
                            s.SessionType,
                            s.PlannedDurationMinutes,
                            s.PlannedDistanceKm,
                            s.DayOfWeek))
                        .ToList()))
                .ToList()));
})
.WithName("GetPlanById");

v1.MapGet("/plans", async (PlanStatus? status, ActivitiesDbContext db) =>
{
    var athlete = await db.AthleteProfiles.AsNoTracking().FirstOrDefaultAsync();
    if (athlete is null)
    {
        return Results.BadRequest("Athlete profile is required before querying plans.");
    }

    var query = db.TrainingPlans.AsNoTracking().Where(p => p.AthleteId == athlete.Id);
    if (status.HasValue)
    {
        query = query.Where(p => p.Status == status.Value);
    }

    var plans = await query
        .OrderByDescending(p => p.CreatedAtUtc)
        .Select(p => new
        {
            p.Id,
            p.GoalId,
            p.Name,
            p.StartDate,
            p.EndDate,
            p.Status
        })
        .ToListAsync();

    return Results.Ok(plans);
})
.WithName("GetPlans");

v1.MapPatch("/plans/{planId:guid}/status", async (Guid planId, UpdatePlanStatusRequest request, ActivitiesDbContext db) =>
{
    var plan = await db.TrainingPlans.FirstOrDefaultAsync(p => p.Id == planId);
    if (plan is null)
    {
        return Results.NotFound();
    }

    var isValidTransition = (plan.Status, request.Status) switch
    {
        (PlanStatus.Draft,  PlanStatus.Active)    => true,
        (PlanStatus.Active, PlanStatus.Completed) => true,
        _ => false
    };

    if (!isValidTransition)
    {
        return Results.Conflict($"Cannot transition plan from {plan.Status} to {request.Status}.");
    }

    plan.Status = request.Status;
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        plan.Id,
        plan.GoalId,
        plan.Name,
        plan.StartDate,
        plan.EndDate,
        plan.Status
    });
})
.WithName("UpdatePlanStatus");

v1.MapGet("/progress/weekly", async (Guid planId, DateOnly weekStart, ActivitiesDbContext db) =>
{
    var plan = await db.TrainingPlans
        .AsNoTracking()
        .Include(p => p.Weeks)
        .ThenInclude(w => w.Sessions)
        .FirstOrDefaultAsync(p => p.Id == planId);

    if (plan is null)
    {
        return Results.NotFound("Plan not found.");
    }

    var week = plan.Weeks.FirstOrDefault(w => w.WeekStartDate == weekStart);
    if (week is null)
    {
        return Results.NotFound("Plan week not found.");
    }

    var weekEnd = weekStart.AddDays(6);

    var completedActivities = await db.Activities
        .AsNoTracking()
        .Where(a => a.Date >= weekStart && a.Date <= weekEnd)
        .ToListAsync();

    var disciplines = Enum.GetValues<ActivityType>()
        .Select(discipline =>
        {
            var plannedMinutes = week.Sessions
                .Where(s => s.Discipline == discipline)
                .Sum(s => s.PlannedDurationMinutes);

            var completedMinutes = completedActivities
                .Where(a => a.Type == discipline)
                .Sum(a => a.DurationMinutes);

            var compliancePercent = plannedMinutes == 0
                ? 0
                : decimal.Round((decimal)completedMinutes / plannedMinutes * 100, 2);

            return new
            {
                Discipline = discipline,
                PlannedMinutes = plannedMinutes,
                CompletedMinutes = completedMinutes,
                CompliancePercent = compliancePercent
            };
        })
        .ToList();

    var totalPlanned = disciplines.Sum(d => d.PlannedMinutes);
    var totalCompleted = disciplines.Sum(d => d.CompletedMinutes);
    var totalCompliance = totalPlanned == 0
        ? 0
        : decimal.Round((decimal)totalCompleted / totalPlanned * 100, 2);

    return Results.Ok(new
    {
        PlanId = plan.Id,
        WeekStartDate = weekStart,
        Disciplines = disciplines,
        Totals = new
        {
            PlannedMinutes = totalPlanned,
            CompletedMinutes = totalCompleted,
            CompliancePercent = totalCompliance
        }
    });
})
.WithName("GetWeeklyProgress");

v1.MapGet("/records", async (ActivityType? discipline, ActivitiesDbContext db) =>
{
    var athlete = await db.AthleteProfiles.AsNoTracking().FirstOrDefaultAsync();
    if (athlete is null)
    {
        return Results.BadRequest("Athlete profile is required before querying records.");
    }

    var query = db.PersonalRecords.AsNoTracking().Where(r => r.AthleteId == athlete.Id);
    if (discipline.HasValue)
    {
        query = query.Where(r => r.Discipline == discipline.Value);
    }

    var records = await query
        .OrderByDescending(r => r.AchievedOn)
        .ThenBy(r => r.Discipline)
        .ToListAsync();

    return Results.Ok(records.Select(r => new PersonalRecordResponse(
        r.Id,
        r.AthleteId,
        r.Discipline,
        r.Metric,
        r.Value,
        r.AchievedOn,
        r.SourceActivityId)));
})
.WithName("GetPersonalRecords");

v1.MapPost("/records", async (CreatePersonalRecordRequest request, ActivitiesDbContext db) =>
{
    var athlete = await db.AthleteProfiles.FirstOrDefaultAsync();
    if (athlete is null)
    {
        return Results.BadRequest("Athlete profile is required before creating records.");
    }

    if (request.Value <= 0)
    {
        return Results.BadRequest("Value must be greater than 0.");
    }

    if (request.SourceActivityId.HasValue)
    {
        var sourceExists = await db.Activities.AnyAsync(a => a.Id == request.SourceActivityId.Value);
        if (!sourceExists)
        {
            return Results.BadRequest("SourceActivityId does not exist.");
        }
    }

    var record = new PersonalRecord
    {
        AthleteId = athlete.Id,
        Discipline = request.Discipline,
        Metric = request.Metric,
        Value = request.Value,
        AchievedOn = request.AchievedOn,
        SourceActivityId = request.SourceActivityId
    };

    db.PersonalRecords.Add(record);
    await db.SaveChangesAsync();

    return Results.Created($"/v1/records/{record.Id}", new PersonalRecordResponse(
        record.Id,
        record.AthleteId,
        record.Discipline,
        record.Metric,
        record.Value,
        record.AchievedOn,
        record.SourceActivityId));
})
.WithName("CreatePersonalRecord");

app.MapDefaultEndpoints();

app.Run();

record CreateActivityRequest(DateOnly Date, ActivityType Type, int DurationMinutes, string? Notes);
record UpsertAthleteProfileRequest(string DisplayName, decimal WeeklyHoursAvailable, DateOnly? PrimaryEventDate);
record CreateGoalRequest(GoalType GoalType, ActivityType? Discipline, decimal? TargetValue, DateOnly TargetDate);
record UpdateGoalStatusRequest(GoalStatus Status);
record CreatePlanRequest(Guid GoalId, string Name, DateOnly StartDate, int WeekCount);
record UpdatePlanStatusRequest(PlanStatus Status);
record CreatePersonalRecordRequest(ActivityType Discipline, PersonalRecordMetric Metric, decimal Value, DateOnly AchievedOn, Guid? SourceActivityId);

record AthleteProfileResponse(Guid Id, string DisplayName, decimal WeeklyHoursAvailable, DateOnly? PrimaryEventDate, DateTime CreatedAtUtc);
record GoalResponse(Guid Id, Guid AthleteId, GoalType GoalType, ActivityType? Discipline, decimal? TargetValue, DateOnly TargetDate, GoalStatus Status, DateTime CreatedAtUtc);
record PlanDetailResponse(Guid Id, Guid AthleteId, Guid? GoalId, string Name, DateOnly StartDate, DateOnly EndDate, PlanStatus Status, DateTime CreatedAtUtc, List<PlanWeekResponse> Weeks);
record PlanWeekResponse(Guid Id, Guid PlanId, int WeekIndex, DateOnly WeekStartDate, string? Notes, List<PlannedSessionResponse> Sessions);
record PlannedSessionResponse(Guid Id, Guid PlanWeekId, ActivityType Discipline, SessionType SessionType, int PlannedDurationMinutes, decimal? PlannedDistanceKm, DayOfWeek DayOfWeek);
record PersonalRecordResponse(Guid Id, Guid AthleteId, ActivityType Discipline, PersonalRecordMetric Metric, decimal Value, DateOnly AchievedOn, Guid? SourceActivityId);


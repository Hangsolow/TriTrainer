namespace TriTrainer.Web;

public enum ActivityType
{
    Run,
    Cycle,
    Swim
}

public enum GoalType
{
    EventFinish,
    DisciplinePerformance,
    Consistency
}

public enum GoalStatus
{
    Draft,
    Active,
    Achieved,
    Archived
}

public enum PlanStatus
{
    Draft,
    Active,
    Completed
}

public enum PersonalRecordMetric
{
    Fastest5k,
    Fastest10k,
    LongestRide,
    LongestSwim
}

public enum SessionType
{
    Endurance,
    Tempo,
    Intervals,
    Recovery
}

public record Activity(Guid Id, DateOnly Date, ActivityType Type, int DurationMinutes, string? Notes, DateTime CreatedAt);

public record CreateActivityRequest(DateOnly Date, ActivityType Type, int DurationMinutes, string? Notes);

public record AthleteProfile(Guid Id, string DisplayName, decimal WeeklyHoursAvailable, DateOnly? PrimaryEventDate, DateTime CreatedAtUtc);

public record UpsertAthleteProfileRequest(string DisplayName, decimal WeeklyHoursAvailable, DateOnly? PrimaryEventDate);

public record Goal(Guid Id, Guid AthleteId, GoalType GoalType, ActivityType? Discipline, decimal? TargetValue, DateOnly TargetDate, GoalStatus Status, DateTime CreatedAtUtc);

public record CreateGoalRequest(GoalType GoalType, ActivityType? Discipline, decimal? TargetValue, DateOnly TargetDate);

public record UpdateGoalStatusRequest(GoalStatus Status);

public record PlanSummary(Guid Id, Guid? GoalId, string Name, DateOnly StartDate, DateOnly EndDate, PlanStatus Status);

public record PlanWeek(Guid Id, Guid PlanId, int WeekIndex, DateOnly WeekStartDate, string? Notes, List<PlannedSession> Sessions);

public record PlannedSession(Guid Id, Guid PlanWeekId, ActivityType Discipline, SessionType SessionType, int PlannedDurationMinutes, decimal? PlannedDistanceKm, DayOfWeek DayOfWeek);

public record PlanDetail(Guid Id, Guid AthleteId, Guid? GoalId, string Name, DateOnly StartDate, DateOnly EndDate, PlanStatus Status, DateTime CreatedAtUtc, List<PlanWeek> Weeks);

public record CreatePlanRequest(Guid GoalId, string Name, DateOnly StartDate, int WeekCount);

public record UpdatePlanStatusRequest(PlanStatus Status);

public record WeeklyProgressDiscipline(ActivityType Discipline, int PlannedMinutes, int CompletedMinutes, decimal CompliancePercent);

public record WeeklyProgressTotals(int PlannedMinutes, int CompletedMinutes, decimal CompliancePercent);

public record WeeklyProgressResponse(Guid PlanId, DateOnly WeekStartDate, List<WeeklyProgressDiscipline> Disciplines, WeeklyProgressTotals Totals);

public record PersonalRecord(Guid Id, Guid AthleteId, ActivityType Discipline, PersonalRecordMetric Metric, decimal Value, DateOnly AchievedOn, Guid? SourceActivityId);

public record CreatePersonalRecordRequest(ActivityType Discipline, PersonalRecordMetric Metric, decimal Value, DateOnly AchievedOn, Guid? SourceActivityId);

public class ActivityApiClient(HttpClient httpClient)
{
    public async Task<List<Activity>> GetActivitiesAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<List<Activity>>(
            $"/activities?year={year}&month={month}", cancellationToken);
        return result ?? [];
    }

    public async Task<Activity?> AddActivityAsync(CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/activities", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Activity>(cancellationToken);
    }

    public async Task DeleteActivityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/activities/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AthleteProfile?> GetAthleteProfileAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("/v1/athlete-profile", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AthleteProfile>(cancellationToken);
    }

    public async Task<AthleteProfile?> UpsertAthleteProfileAsync(UpsertAthleteProfileRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync("/v1/athlete-profile", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AthleteProfile>(cancellationToken);
    }

    public async Task<List<Goal>> GetGoalsAsync(GoalStatus? status = null, CancellationToken cancellationToken = default)
    {
        var path = status.HasValue ? $"/v1/goals?status={status.Value}" : "/v1/goals";
        var response = await httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Goal>>(cancellationToken);
        return result ?? [];
    }

    public async Task<Goal?> CreateGoalAsync(CreateGoalRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/v1/goals", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Goal>(cancellationToken);
    }

    public async Task<Goal?> UpdateGoalStatusAsync(Guid goalId, GoalStatus status, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsJsonAsync($"/v1/goals/{goalId}/status", new UpdateGoalStatusRequest(status), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Goal>(cancellationToken);
    }

    public async Task<List<PlanSummary>> GetPlansAsync(PlanStatus? status = null, CancellationToken cancellationToken = default)
    {
        var path = status.HasValue ? $"/v1/plans?status={status.Value}" : "/v1/plans";
        var response = await httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<PlanSummary>>(cancellationToken);
        return result ?? [];
    }

    public async Task<PlanDetail?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/v1/plans/{planId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanDetail>(cancellationToken);
    }

    public async Task<PlanSummary?> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/v1/plans", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanSummary>(cancellationToken);
    }

    public async Task<PlanSummary?> UpdatePlanStatusAsync(Guid planId, PlanStatus status, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsJsonAsync($"/v1/plans/{planId}/status", new UpdatePlanStatusRequest(status), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanSummary>(cancellationToken);
    }

    public async Task<WeeklyProgressResponse?> GetWeeklyProgressAsync(Guid planId, DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/v1/progress/weekly?planId={planId}&weekStart={weekStart:yyyy-MM-dd}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WeeklyProgressResponse>(cancellationToken);
    }

    public async Task<List<PersonalRecord>> GetRecordsAsync(ActivityType? discipline = null, CancellationToken cancellationToken = default)
    {
        var path = discipline.HasValue ? $"/v1/records?discipline={discipline.Value}" : "/v1/records";
        var response = await httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<PersonalRecord>>(cancellationToken);
        return result ?? [];
    }

    public async Task<PersonalRecord?> CreateRecordAsync(CreatePersonalRecordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/v1/records", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PersonalRecord>(cancellationToken);
    }
}

using System.Net.Http.Json;
using TUnit.Aspire;

namespace TriTrainer.IntegrationTests;

public class AppFixture : AspireFixture<Projects.TriTrainer_AppHost>
{
    protected override TimeSpan ResourceTimeout => TimeSpan.FromSeconds(GetResourceTimeoutSeconds());

    static AppFixture()
    {
        Environment.SetEnvironmentVariable("TRITRAINER_DISABLE_PGADMIN", "true");
    }

    private static int GetResourceTimeoutSeconds()
    {
        const int defaultTimeoutSeconds = 60;

        var raw = Environment.GetEnvironmentVariable("TRITRAINER_ASPIRE_STARTUP_TIMEOUT_SECONDS");

        if (int.TryParse(raw, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        return defaultTimeoutSeconds;
    }

    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public class CalendarApiTests(AppFixture fixture)
{
    [Test]
    public async Task GetActivities_ReturnsOk()
    {
        var client = fixture.CreateHttpClient("apiservice");
        var response = await client.GetAsync("/activities?year=2026&month=4");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task PostActivity_ReturnsCreated()
    {
        var client = fixture.CreateHttpClient("apiservice");

        var response = await client.PostAsJsonAsync("/activities", new
        {
            date = "2026-04-30",
            type = "Run",
            durationMinutes = 45,
            notes = "Integration test run"
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
    }

    [Test]
    public async Task PostActivity_ThenAppearInGetActivities()
    {
        var client = fixture.CreateHttpClient("apiservice");

        var date = DateOnly.FromDateTime(DateTime.Today);

        var createResponse = await client.PostAsJsonAsync("/activities", new
        {
            date = date.ToString("yyyy-MM-dd"),
            type = "Swim",
            durationMinutes = 30,
            notes = "Integration test swim"
        });

        createResponse.EnsureSuccessStatusCode();

        var activities = await client
            .GetFromJsonAsync<List<ActivityResponse>>($"/activities?year={date.Year}&month={date.Month}");

        await Assert.That(activities).IsNotNull();
        await Assert.That(activities!.Any(a => a.Notes == "Integration test swim")).IsTrue();
    }

    [Test]
    public async Task DeleteActivity_ReturnsNoContent()
    {
        var client = fixture.CreateHttpClient("apiservice");

        var createResponse = await client.PostAsJsonAsync("/activities", new
        {
            date = "2026-04-30",
            type = "Cycle",
            durationMinutes = 60,
            notes = "To be deleted"
        });

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ActivityResponse>();

        var deleteResponse = await client.DeleteAsync($"/activities/{created!.Id}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task WebFrontend_ReturnsOk()
    {
        var client = fixture.CreateHttpClient("webfrontend");
        var response = await client.GetAsync("/");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    record ActivityResponse(Guid Id, string Date, string Type, int DurationMinutes, string? Notes);
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class StartupHealthRegressionTests(AppFixture fixture)
{
    [Test]
    public async Task ApiService_HealthAndAliveEndpoints_ReturnOk()
    {
        var apiClient = fixture.CreateHttpClient("apiservice");

        var healthResponse = await GetUntilOkAsync(apiClient, "/health", TimeSpan.FromSeconds(30));
        var healthBody = await healthResponse.Content.ReadAsStringAsync();

        await Assert.That(healthResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(healthBody).Contains("Healthy");

        var aliveResponse = await apiClient.GetAsync("/alive");
        var aliveBody = await aliveResponse.Content.ReadAsStringAsync();

        await Assert.That(aliveResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(aliveBody).Contains("Healthy");
    }

    [Test]
    public async Task WebFrontend_HealthAndAliveEndpoints_ReturnOk()
    {
        var webClient = fixture.CreateHttpClient("webfrontend");

        var healthResponse = await GetUntilOkAsync(webClient, "/health", TimeSpan.FromSeconds(30));
        var healthBody = await healthResponse.Content.ReadAsStringAsync();

        await Assert.That(healthResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(healthBody).Contains("Healthy");

        var aliveResponse = await webClient.GetAsync("/alive");
        var aliveBody = await aliveResponse.Content.ReadAsStringAsync();

        await Assert.That(aliveResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(aliveBody).Contains("Healthy");
    }

    [Test]
    public async Task CrossServiceStartupReadiness_HealthProbesThenCoreEndpoints_ReturnOk()
    {
        var apiClient = fixture.CreateHttpClient("apiservice");
        var webClient = fixture.CreateHttpClient("webfrontend");

        _ = await GetUntilOkAsync(apiClient, "/health", TimeSpan.FromSeconds(30));
        _ = await GetUntilOkAsync(webClient, "/health", TimeSpan.FromSeconds(30));

        var webResponse = await webClient.GetAsync("/");
        await Assert.That(webResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var apiResponse = await apiClient.GetAsync("/activities?year=2026&month=4");
        await Assert.That(apiResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    private static async Task<HttpResponseMessage> GetUntilOkAsync(HttpClient client, string path, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        HttpResponseMessage? lastResponse = null;

        while (DateTime.UtcNow - start < timeout)
        {
            lastResponse?.Dispose();
            lastResponse = await client.GetAsync(path);

            if (lastResponse.StatusCode == HttpStatusCode.OK)
            {
                return lastResponse;
            }

            await Task.Delay(250);
        }

        return lastResponse ?? new HttpResponseMessage(HttpStatusCode.RequestTimeout);
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class RecordsApiRegressionTests(AppFixture fixture)
{
    [Test]
    public async Task GetRecords_DisciplineFilter_ReturnsOnlyRequestedDiscipline()
    {
        var client = fixture.CreateHttpClient("apiservice");
        await TestDataSeeder.EnsureAthleteProfileAsync(client, "Records Filter Athlete");

        var runRecordId = await TestDataSeeder.CreateRecordAsync(client, "Run", "Fastest5k", 24.30m, DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-10));
        _ = await TestDataSeeder.CreateRecordAsync(client, "Cycle", "LongestRide", 42.00m, DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-9));
        var secondRunRecordId = await TestDataSeeder.CreateRecordAsync(client, "Run", "Fastest10k", 53.10m, DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-8));

        var response = await client.GetAsync("/v1/records?discipline=Run");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var records = await response.Content.ReadFromJsonAsync<List<PersonalRecordResponse>>();
        await Assert.That(records).IsNotNull();
        await Assert.That(records!.Any(r => r.Id == runRecordId)).IsTrue();
        await Assert.That(records.Any(r => r.Id == secondRunRecordId)).IsTrue();
        await Assert.That(records.All(r => r.Discipline == "Run")).IsTrue();
    }

    [Test]
    public async Task GetPersonalBest_ReturnsBestValues_AndSupportsDisciplineFilter()
    {
        var client = fixture.CreateHttpClient("apiservice");
        await TestDataSeeder.EnsureAthleteProfileAsync(client, "Records PB Athlete");

        _ = await TestDataSeeder.CreateRecordAsync(client, "Run", "Fastest5k", 25.00m, DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-20));
        _ = await TestDataSeeder.CreateRecordAsync(client, "Run", "Fastest5k", 23.40m, DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-19));
        _ = await TestDataSeeder.CreateRecordAsync(client, "Cycle", "LongestRide", 70.00m, DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-18));
        _ = await TestDataSeeder.CreateRecordAsync(client, "Cycle", "LongestRide", 90.00m, DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-17));

        var allResponse = await client.GetAsync("/v1/records/personal-best");
        await Assert.That(allResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var allPersonalBests = await allResponse.Content.ReadFromJsonAsync<List<PersonalRecordResponse>>();

        await Assert.That(allPersonalBests).IsNotNull();
        var runBest = allPersonalBests!.Single(r => r.Discipline == "Run" && r.Metric == "Fastest5k");
        var cycleBest = allPersonalBests.Single(r => r.Discipline == "Cycle" && r.Metric == "LongestRide");
        await Assert.That(runBest.Value).IsEqualTo(23.40m);
        await Assert.That(cycleBest.Value).IsEqualTo(90.00m);

        var runOnlyResponse = await client.GetAsync("/v1/records/personal-best?discipline=Run");
        await Assert.That(runOnlyResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var runOnlyPersonalBests = await runOnlyResponse.Content.ReadFromJsonAsync<List<PersonalRecordResponse>>();

        await Assert.That(runOnlyPersonalBests).IsNotNull();
        await Assert.That(runOnlyPersonalBests!.Count).IsGreaterThan(0);
        await Assert.That(runOnlyPersonalBests.All(r => r.Discipline == "Run")).IsTrue();
        await Assert.That(runOnlyPersonalBests.Any(r => r.Metric == "Fastest5k" && r.Value == 23.40m)).IsTrue();
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class RecordsApiEmptyDatasetTests(AppFixture fixture)
{
    [Test]
    public async Task GetPersonalBest_WithNoRecords_ReturnsEmptyArray()
    {
        var client = fixture.CreateHttpClient("apiservice");
        await TestDataSeeder.EnsureAthleteProfileAsync(client, "Records Empty Dataset Athlete");

        var response = await client.GetAsync("/v1/records/personal-best");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var records = await response.Content.ReadFromJsonAsync<List<PersonalRecordResponse>>();
        await Assert.That(records).IsNotNull();
        await Assert.That(records!.Count).IsEqualTo(0);
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class ProgressSummaryWeeksValidationTests(AppFixture fixture)
{
    [Test]
    public async Task ProgressSummary_WeeksBounds_AreEnforced_AndDefaultPathIsHealthy()
    {
        var client = fixture.CreateHttpClient("apiservice");

        var belowRange = await client.GetAsync("/v1/progress/summary?weeks=0");
        await Assert.That(belowRange.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var aboveRange = await client.GetAsync("/v1/progress/summary?weeks=53");
        await Assert.That(aboveRange.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        _ = await TestDataSeeder.CreateActivePlanForRecentWeeksAsync(client, "Weeks Bounds Athlete", 4);

        var inRange = await client.GetAsync("/v1/progress/summary?weeks=4");
        await Assert.That(inRange.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class ProgressSummaryNoActivityEdgeCaseTests(AppFixture fixture)
{
    [Test]
    public async Task ProgressSummary_NoCompletedActivities_ReturnsZeroComplianceAndZeroStreaks()
    {
        var client = fixture.CreateHttpClient("apiservice");
        _ = await TestDataSeeder.CreateActivePlanForRecentWeeksAsync(client, "No Activity Athlete", 4);

        var response = await client.GetAsync("/v1/progress/summary?weeks=4");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<ProgressSummaryResponse>();
        await Assert.That(summary).IsNotNull();
        await Assert.That(summary!.WeeksSummary.Count).IsEqualTo(4);
        await Assert.That(summary.WeeksSummary.All(w => w.CompliancePercent == 0)).IsTrue();
        await Assert.That(summary.OverallCompliancePercent).IsEqualTo(0);
        await Assert.That(summary.CurrentStreakWeeks).IsEqualTo(0);
        await Assert.That(summary.LongestStreakWeeks).IsEqualTo(0);
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class ProgressSummaryAllAboveThresholdEdgeCaseTests(AppFixture fixture)
{
    [Test]
    public async Task ProgressSummary_AllWeeksAboveThreshold_ReturnsFullCurrentAndLongestStreaks()
    {
        var client = fixture.CreateHttpClient("apiservice");
        var seed = await TestDataSeeder.CreateActivePlanForRecentWeeksAsync(client, "All Above Threshold Athlete", 4);

        foreach (var weekStart in seed.WeekStarts)
        {
            await TestDataSeeder.CreateActivityAsync(client, weekStart, "Run", 10000, "All-above-threshold coverage activity");
        }

        var response = await client.GetAsync("/v1/progress/summary?weeks=4");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<ProgressSummaryResponse>();
        await Assert.That(summary).IsNotNull();
        await Assert.That(summary!.WeeksSummary.Count).IsEqualTo(4);
        await Assert.That(summary.WeeksSummary.All(w => w.CompliancePercent >= 70)).IsTrue();
        await Assert.That(summary.CurrentStreakWeeks).IsEqualTo(4);
        await Assert.That(summary.LongestStreakWeeks).IsEqualTo(4);
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class ProgressSummaryMixedStreakEdgeCaseTests(AppFixture fixture)
{
    [Test]
    public async Task ProgressSummary_MixedCompliance_ComputesOverallAndStreaksCorrectly()
    {
        var client = fixture.CreateHttpClient("apiservice");
        var seed = await TestDataSeeder.CreateActivePlanForRecentWeeksAsync(client, "Mixed Streak Athlete", 4);

        await TestDataSeeder.CreateActivityAsync(client, seed.WeekStarts[0], "Run", seed.PlannedByWeek[seed.WeekStarts[0]], "Week1 full");
        await TestDataSeeder.CreateActivityAsync(client, seed.WeekStarts[2], "Cycle", seed.PlannedByWeek[seed.WeekStarts[2]], "Week3 full");
        await TestDataSeeder.CreateActivityAsync(client, seed.WeekStarts[3], "Swim", seed.PlannedByWeek[seed.WeekStarts[3]], "Week4 full");

        var response = await client.GetAsync("/v1/progress/summary?weeks=4");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<ProgressSummaryResponse>();
        await Assert.That(summary).IsNotNull();

        var totalPlanned = summary!.WeeksSummary.Sum(w => w.PlannedMinutes);
        var totalCompleted = summary.WeeksSummary.Sum(w => w.CompletedMinutes);
        var expectedOverall = totalPlanned == 0
            ? 0
            : decimal.Round((decimal)totalCompleted / totalPlanned * 100, 2);

        await Assert.That(summary.CurrentStreakWeeks).IsEqualTo(2);
        await Assert.That(summary.LongestStreakWeeks).IsEqualTo(2);
        await Assert.That(summary.OverallCompliancePercent).IsEqualTo(expectedOverall);
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class RecommendationInsightsApiTests(AppFixture fixture)
{
    [Test]
    public async Task RecommendationInsights_WithActivePlan_ReturnsActionableInsights()
    {
        var client = fixture.CreateHttpClient("apiservice");
        _ = await TestDataSeeder.CreateActivePlanForRecentWeeksAsync(client, "Recommendation Athlete", 4);

        var response = await client.GetAsync("/v1/recommendations/insights?weeks=4");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<RecommendationInsightsResponse>();
        await Assert.That(payload).IsNotNull();
        await Assert.That(payload!.WeeksEvaluated).IsEqualTo(4);
        await Assert.That(payload.Insights.Count).IsGreaterThan(0);
        await Assert.That(payload.Insights.Any(i => i.Code == "consistency_recovery")).IsTrue();
        await Assert.That(payload.Insights.Any(i => i.Code == "plan_next_session")).IsTrue();
    }

    [Test]
    public async Task RecommendationInsights_WeeksBounds_AreEnforced()
    {
        var client = fixture.CreateHttpClient("apiservice");

        var belowRange = await client.GetAsync("/v1/recommendations/insights?weeks=0");
        await Assert.That(belowRange.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        var aboveRange = await client.GetAsync("/v1/recommendations/insights?weeks=13");
        await Assert.That(aboveRange.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}

[ClassDataSource<AppFixture>(Shared = SharedType.PerClass)]
public class GoalQuickStartApiTests(AppFixture fixture)
{
    [Test]
    public async Task CreateQuickStartGoal_CreatesActiveGoalAndPlanInSingleCall()
    {
        var client = fixture.CreateHttpClient("apiservice");
        await TestDataSeeder.EnsureAthleteProfileAsync(client, "Quick Start Athlete");

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var startDate = today.AddDays(7);
        var targetDate = startDate.AddDays(7 * 10);

        var response = await client.PostAsJsonAsync("/v1/goals/quick-start", new
        {
            goalType = "EventFinish",
            discipline = (string?)null,
            targetValue = (decimal?)null,
            targetDate = targetDate.ToString("yyyy-MM-dd"),
            planName = "Quick Start Build",
            startDate = startDate.ToString("yyyy-MM-dd"),
            weekCount = 8,
            activatePlan = true
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<QuickStartGoalResponse>();
        await Assert.That(created).IsNotNull();
        await Assert.That(created!.Goal.Status).IsEqualTo("Active");
        await Assert.That(created.Plan.Status).IsEqualTo("Active");
        await Assert.That(created.Plan.WeekCount).IsEqualTo(8);
        await Assert.That(created.Plan.SessionCount).IsGreaterThan(0);
    }

    [Test]
    public async Task CreateQuickStartGoal_InvalidRange_ReturnsBadRequest()
    {
        var client = fixture.CreateHttpClient("apiservice");
        await TestDataSeeder.EnsureAthleteProfileAsync(client, "Quick Start Validation Athlete");

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var startDate = today.AddDays(7);

        var response = await client.PostAsJsonAsync("/v1/goals/quick-start", new
        {
            goalType = "EventFinish",
            discipline = (string?)null,
            targetValue = (decimal?)null,
            targetDate = startDate.AddDays(7 * 4).ToString("yyyy-MM-dd"),
            planName = "Too Long",
            startDate = startDate.ToString("yyyy-MM-dd"),
            weekCount = 16,
            activatePlan = false
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}

public static class TestDataSeeder
{
    public static async Task EnsureAthleteProfileAsync(HttpClient client, string displayName)
    {
        var response = await client.PutAsJsonAsync("/v1/athlete-profile", new
        {
            displayName,
            weeklyHoursAvailable = 8.0m,
            primaryEventDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddMonths(4).ToString("yyyy-MM-dd")
        });

        response.EnsureSuccessStatusCode();
    }

    public static async Task<Guid> CreateRecordAsync(HttpClient client, string discipline, string metric, decimal value, DateOnly achievedOn)
    {
        var response = await client.PostAsJsonAsync("/v1/records", new
        {
            discipline,
            metric,
            value,
            achievedOn = achievedOn.ToString("yyyy-MM-dd")
        });

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedRecordResponse>();
        return created!.Id;
    }

    public static async Task CreateActivityAsync(HttpClient client, DateOnly date, string type, int durationMinutes, string notes)
    {
        var response = await client.PostAsJsonAsync("/activities", new
        {
            date = date.ToString("yyyy-MM-dd"),
            type,
            durationMinutes,
            notes
        });

        response.EnsureSuccessStatusCode();
    }

    public static async Task<RecentPlanSeedResult> CreateActivePlanForRecentWeeksAsync(HttpClient client, string displayName, int weekCount)
    {
        await EnsureAthleteProfileAsync(client, displayName);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var currentWeekStart = today.AddDays(-daysSinceMonday);
        var mostRecentCompletedWeekStart = currentWeekStart.AddDays(-7);
        var startDate = mostRecentCompletedWeekStart.AddDays(-(weekCount - 1) * 7);

        var goalResponse = await client.PostAsJsonAsync("/v1/goals", new
        {
            goalType = "EventFinish",
            discipline = (string?)null,
            targetValue = (decimal?)null,
            targetDate = today.AddMonths(6).ToString("yyyy-MM-dd")
        });

        goalResponse.EnsureSuccessStatusCode();
        var goal = await goalResponse.Content.ReadFromJsonAsync<GoalCreateResponse>();

        var planResponse = await client.PostAsJsonAsync("/v1/plans", new
        {
            goalId = goal!.Id,
            name = $"Regression Plan {Guid.NewGuid():N}",
            startDate = startDate.ToString("yyyy-MM-dd"),
            weekCount
        });

        planResponse.EnsureSuccessStatusCode();
        var plan = await planResponse.Content.ReadFromJsonAsync<PlanCreateResponse>();

        var activateResponse = await client.PatchAsJsonAsync($"/v1/plans/{plan!.Id}/status", new
        {
            status = "Active"
        });
        activateResponse.EnsureSuccessStatusCode();

        var detailResponse = await client.GetAsync($"/v1/plans/{plan.Id}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<PlanDetailResponse>();

        var orderedWeeks = detail!.Weeks.OrderBy(w => w.WeekStartDate).ToList();
        var plannedByWeek = orderedWeeks.ToDictionary(
            w => w.WeekStartDate,
            w => w.Sessions.Sum(s => s.PlannedDurationMinutes));

        return new RecentPlanSeedResult(plan.Id, orderedWeeks.Select(w => w.WeekStartDate).ToList(), plannedByWeek);
    }
}

public record PersonalRecordResponse(Guid Id, string Discipline, string Metric, decimal Value);
public record ProgressSummaryResponse(Guid PlanId, List<WeeklyComplianceSummaryResponse> WeeksSummary, decimal OverallCompliancePercent, int CurrentStreakWeeks, int LongestStreakWeeks);
public record WeeklyComplianceSummaryResponse(DateOnly WeekStartDate, int PlannedMinutes, int CompletedMinutes, decimal CompliancePercent);
public record CreatedRecordResponse(Guid Id);
public record GoalCreateResponse(Guid Id);
public record PlanCreateResponse(Guid Id);
public record PlanDetailResponse(List<PlanWeekDetailResponse> Weeks);
public record PlanWeekDetailResponse(DateOnly WeekStartDate, List<PlanSessionDetailResponse> Sessions);
public record PlanSessionDetailResponse(int PlannedDurationMinutes);
public record RecentPlanSeedResult(Guid PlanId, List<DateOnly> WeekStarts, Dictionary<DateOnly, int> PlannedByWeek);
public record RecommendationInsightsResponse(Guid? PlanId, int WeeksEvaluated, decimal OverallCompliancePercent, int CurrentStreakWeeks, NextPlannedSessionResponse? NextPlannedSession, List<RecommendationInsightResponse> Insights);
public record RecommendationInsightResponse(string Code, string Title, string Message, string Severity, string Action);
public record NextPlannedSessionResponse(DateOnly Date, string Discipline, string SessionType, int PlannedDurationMinutes);
public record QuickStartGoalResponse(QuickStartGoalItemResponse Goal, QuickStartPlanItemResponse Plan);
public record QuickStartGoalItemResponse(Guid Id, string Status);
public record QuickStartPlanItemResponse(Guid Id, string Status, int WeekCount, int SessionCount);

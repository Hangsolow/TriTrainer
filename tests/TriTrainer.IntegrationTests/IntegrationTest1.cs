using System.Net.Http.Json;
using TUnit.Aspire;

namespace TriTrainer.IntegrationTests;

public class AppFixture : AspireFixture<Projects.TriTrainer_AppHost>
{
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

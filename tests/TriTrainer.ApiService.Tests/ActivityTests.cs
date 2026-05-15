using Microsoft.EntityFrameworkCore;
using TriTrainer.ApiService.Data;

namespace TriTrainer.ApiService.Tests;

public class ActivityTests
{
    private static ActivitiesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ActivitiesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ActivitiesDbContext(options);
    }

    [Test]
    public async Task Activity_CanBeAdded_AndRetrieved()
    {
        await using var db = CreateDb();

        var activity = new Activity
        {
            Date = new DateOnly(2026, 4, 30),
            Type = ActivityType.Run,
            DurationMinutes = 45,
            Notes = "Easy run"
        };

        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        var stored = await db.Activities.FindAsync(activity.Id);

        await Assert.That(stored).IsNotNull();
        await Assert.That(stored!.Type).IsEqualTo(ActivityType.Run);
        await Assert.That(stored.DurationMinutes).IsEqualTo(45);
        await Assert.That(stored.Notes).IsEqualTo("Easy run");
    }

    [Test]
    public async Task Activities_CanBeFilteredByMonth()
    {
        await using var db = CreateDb();

        db.Activities.AddRange(
            new Activity { Date = new DateOnly(2026, 4, 1), Type = ActivityType.Run, DurationMinutes = 30 },
            new Activity { Date = new DateOnly(2026, 4, 15), Type = ActivityType.Cycle, DurationMinutes = 60 },
            new Activity { Date = new DateOnly(2026, 5, 1), Type = ActivityType.Swim, DurationMinutes = 40 }
        );
        await db.SaveChangesAsync();

        var from = new DateOnly(2026, 4, 1);
        var to = new DateOnly(2026, 4, 30);
        var aprilActivities = await db.Activities
            .Where(a => a.Date >= from && a.Date <= to)
            .ToListAsync();

        await Assert.That(aprilActivities.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Activity_CanBeDeleted()
    {
        await using var db = CreateDb();

        var activity = new Activity
        {
            Date = new DateOnly(2026, 4, 30),
            Type = ActivityType.Swim,
            DurationMinutes = 30
        };

        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        db.Activities.Remove(activity);
        await db.SaveChangesAsync();

        var result = await db.Activities.FindAsync(activity.Id);
        await Assert.That(result).IsNull();
    }

    [Test]
    [Arguments(ActivityType.Run)]
    [Arguments(ActivityType.Cycle)]
    [Arguments(ActivityType.Swim)]
    public async Task Activity_SupportsAllActivityTypes(ActivityType type)
    {
        await using var db = CreateDb();

        var activity = new Activity
        {
            Date = new DateOnly(2026, 4, 30),
            Type = type,
            DurationMinutes = 60
        };

        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        var stored = await db.Activities.FindAsync(activity.Id);
        await Assert.That(stored!.Type).IsEqualTo(type);
    }

    [Test]
    public async Task Activity_CanHaveNullNotes()
    {
        await using var db = CreateDb();

        var activity = new Activity
        {
            Date = new DateOnly(2026, 4, 30),
            Type = ActivityType.Run,
            DurationMinutes = 45,
            Notes = null
        };

        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        var stored = await db.Activities.FindAsync(activity.Id);
        await Assert.That(stored!.Notes).IsNull();
    }
}

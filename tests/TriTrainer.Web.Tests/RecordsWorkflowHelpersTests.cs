using TriTrainer.Web.Components.Pages;

namespace TriTrainer.Web.Tests;

public class RecordsWorkflowHelpersTests
{
    [Test]
    public async Task DefaultMetricForDiscipline_ReturnsExpectedMetricForEachDiscipline()
    {
        var runMetric = RecordsWorkflowHelpers.DefaultMetricForDiscipline(ActivityType.Run);
        var cycleMetric = RecordsWorkflowHelpers.DefaultMetricForDiscipline(ActivityType.Cycle);
        var swimMetric = RecordsWorkflowHelpers.DefaultMetricForDiscipline(ActivityType.Swim);

        await Assert.That(runMetric).IsEqualTo(PersonalRecordMetric.Fastest5k);
        await Assert.That(cycleMetric).IsEqualTo(PersonalRecordMetric.LongestRide);
        await Assert.That(swimMetric).IsEqualTo(PersonalRecordMetric.LongestSwim);
    }

    [Test]
    public async Task QuickDatePreset_ResolvesTodayYesterdayAndFallback()
    {
        var reference = new DateTime(2026, 05, 24, 18, 30, 0, DateTimeKind.Local);

        var today = RecordsWorkflowHelpers.QuickDatePreset("today", reference);
        var yesterday = RecordsWorkflowHelpers.QuickDatePreset("yesterday", reference);
        var fallback = RecordsWorkflowHelpers.QuickDatePreset("unexpected", reference);

        await Assert.That(today).IsEqualTo(new DateTime(2026, 05, 24));
        await Assert.That(yesterday).IsEqualTo(new DateTime(2026, 05, 23));
        await Assert.That(fallback).IsEqualTo(new DateTime(2026, 05, 24));
    }
}

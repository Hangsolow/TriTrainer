using TriTrainer.Web.Components.Pages;

namespace TriTrainer.Web.Tests;

public class DashboardRecommendationHelpersTests
{
    [Test]
    public async Task InsightsOrEmpty_ReturnsEmpty_WhenResponseIsNull()
    {
        var result = DashboardRecommendationHelpers.InsightsOrEmpty(null);

        await Assert.That(result.Count).IsEqualTo(0);
    }

    [Test]
    public async Task InsightsOrEmpty_ReturnsEmpty_WhenInsightsIsNull()
    {
        var response = new RecommendationInsightsResponse(
            null,
            4,
            82,
            2,
            null,
            null!);

        var result = DashboardRecommendationHelpers.InsightsOrEmpty(response);

        await Assert.That(result.Count).IsEqualTo(0);
    }

    [Test]
    public async Task InsightsOrEmpty_ReturnsInsights_WhenInsightsExist()
    {
        var insights = new List<RecommendationInsight>
        {
            new("consistency_build", "Build consistency", "Keep momentum", "medium", "Open progress")
        };

        var response = new RecommendationInsightsResponse(
            null,
            4,
            82,
            2,
            null,
            insights);

        var result = DashboardRecommendationHelpers.InsightsOrEmpty(response);

        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(result[0].Code).IsEqualTo("consistency_build");
    }

    [Test]
    public async Task SeverityBadgeClass_UsesLowFallback_ForNullAndUnknown()
    {
        var nullSeverityClass = DashboardRecommendationHelpers.SeverityBadgeClass(null);
        var unknownSeverityClass = DashboardRecommendationHelpers.SeverityBadgeClass("unexpected");

        await Assert.That(nullSeverityClass).IsEqualTo("tt-severity-low");
        await Assert.That(unknownSeverityClass).IsEqualTo("tt-severity-low");
    }

    [Test]
    public async Task SeverityLabel_UsesLowFallback_ForNullAndWhitespace()
    {
        var nullSeverityLabel = DashboardRecommendationHelpers.SeverityLabel(null);
        var whitespaceSeverityLabel = DashboardRecommendationHelpers.SeverityLabel("   ");

        await Assert.That(nullSeverityLabel).IsEqualTo("Low");
        await Assert.That(whitespaceSeverityLabel).IsEqualTo("Low");
    }

    [Test]
    public async Task RecommendationHref_UsesCalendarFallback_ForUnknownOrEmpty()
    {
        var unknownHref = DashboardRecommendationHelpers.RecommendationHref("unexpected");
        var emptyHref = DashboardRecommendationHelpers.RecommendationHref(string.Empty);

        await Assert.That(unknownHref).IsEqualTo("calendar");
        await Assert.That(emptyHref).IsEqualTo("calendar");
    }

    [Test]
    public async Task RecommendationCtaLabel_UsesDefault_ForUnknownOrNull()
    {
        var unknownLabel = DashboardRecommendationHelpers.RecommendationCtaLabel("unexpected");
        var nullLabel = DashboardRecommendationHelpers.RecommendationCtaLabel(null);

        await Assert.That(unknownLabel).IsEqualTo("Open calendar");
        await Assert.That(nullLabel).IsEqualTo("Open calendar");
    }
}

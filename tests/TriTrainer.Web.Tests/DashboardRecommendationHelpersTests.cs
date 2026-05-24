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

    [Test]
    public async Task RecommendationHrefWithContext_UsesProgressDeepLink_ForProgressCodes_WhenContextExists()
    {
        var planId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var weekStart = new DateOnly(2026, 05, 18);

        var href = DashboardRecommendationHelpers.RecommendationHrefWithContext("consistency_build", planId, weekStart);

        await Assert.That(href).IsEqualTo("progress?planId=11111111-1111-1111-1111-111111111111&weekStart=2026-05-18");
    }

    [Test]
    public async Task RecommendationHrefWithContext_UsesPlansRoute_ForPlanFocusedCodes()
    {
        var planId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var weekStart = new DateOnly(2026, 05, 18);

        var href = DashboardRecommendationHelpers.RecommendationHrefWithContext("activate_plan", planId, weekStart);

        await Assert.That(href).IsEqualTo("plans");
    }

    [Test]
    public async Task RecommendationHrefWithContext_UsesNonContextFallback_WhenContextMissing()
    {
        var missingPlanHref = DashboardRecommendationHelpers.RecommendationHrefWithContext("consistency_build", null, new DateOnly(2026, 05, 18));
        var missingWeekHref = DashboardRecommendationHelpers.RecommendationHrefWithContext("streak_keepalive", Guid.Parse("11111111-1111-1111-1111-111111111111"), null);

        await Assert.That(missingPlanHref).IsEqualTo("progress");
        await Assert.That(missingWeekHref).IsEqualTo("progress");
    }

    [Test]
    public async Task RecommendationHrefWithContext_UsesCalendarFallback_ForUnknownOrEmpty()
    {
        var unknownHref = DashboardRecommendationHelpers.RecommendationHrefWithContext("unexpected", Guid.Parse("11111111-1111-1111-1111-111111111111"), new DateOnly(2026, 05, 18));
        var emptyHref = DashboardRecommendationHelpers.RecommendationHrefWithContext(string.Empty, Guid.Parse("11111111-1111-1111-1111-111111111111"), new DateOnly(2026, 05, 18));

        await Assert.That(unknownHref).IsEqualTo("calendar");
        await Assert.That(emptyHref).IsEqualTo("calendar");
    }

    [Test]
    public async Task TrendWindowLabel_UsesSingularAndPluralFormatting()
    {
        var singularLabel = DashboardRecommendationHelpers.TrendWindowLabel(1);
        var pluralLabel = DashboardRecommendationHelpers.TrendWindowLabel(4);

        await Assert.That(singularLabel).IsEqualTo("latest 1-week trend");
        await Assert.That(pluralLabel).IsEqualTo("latest 4-week trend");
    }

    [Test]
    public async Task ComplianceBandClass_MapsThresholdsAndNullToExpectedClass()
    {
        var nullClass = DashboardRecommendationHelpers.ComplianceBandClass(null);
        var lowClass = DashboardRecommendationHelpers.ComplianceBandClass(69);
        var okClass = DashboardRecommendationHelpers.ComplianceBandClass(70);
        var goodClass = DashboardRecommendationHelpers.ComplianceBandClass(90);

        await Assert.That(nullClass).IsEqualTo("tt-variance-neutral");
        await Assert.That(lowClass).IsEqualTo("tt-compliance-low");
        await Assert.That(okClass).IsEqualTo("tt-compliance-ok");
        await Assert.That(goodClass).IsEqualTo("tt-compliance-good");
    }

    [Test]
    public async Task ComplianceBandLabel_MapsThresholdsAndNullToExpectedLabel()
    {
        var nullLabel = DashboardRecommendationHelpers.ComplianceBandLabel(null);
        var lowLabel = DashboardRecommendationHelpers.ComplianceBandLabel(45);
        var okLabel = DashboardRecommendationHelpers.ComplianceBandLabel(80);
        var goodLabel = DashboardRecommendationHelpers.ComplianceBandLabel(95);

        await Assert.That(nullLabel).IsEqualTo("No data");
        await Assert.That(lowLabel).IsEqualTo("Needs attention");
        await Assert.That(okLabel).IsEqualTo("Needs a push");
        await Assert.That(goodLabel).IsEqualTo("On track");
    }
}

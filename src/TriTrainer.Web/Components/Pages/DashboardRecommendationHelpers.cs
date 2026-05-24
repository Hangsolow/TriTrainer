namespace TriTrainer.Web.Components.Pages;

internal static class DashboardRecommendationHelpers
{
    internal static IReadOnlyList<RecommendationInsight> InsightsOrEmpty(RecommendationInsightsResponse? recommendationInsights)
    {
        if (recommendationInsights?.Insights is null || recommendationInsights.Insights.Count == 0)
        {
            return [];
        }

        return recommendationInsights.Insights;
    }

    internal static string SeverityBadgeClass(string? severity) => NormalizeSeverity(severity) switch
    {
        "high" => "tt-severity-high",
        "medium" => "tt-severity-medium",
        _ => "tt-severity-low"
    };

    internal static string SeverityLabel(string? severity) => NormalizeSeverity(severity) switch
    {
        "high" => "High",
        "medium" => "Medium",
        _ => "Low"
    };

    internal static string RecommendationHref(string? code) => NormalizeCode(code) switch
    {
        "activate_plan" => "plans",
        "plan_next_session" => "plans",
        "next_session_gap" => "plans",
        "consistency_recovery" => "progress",
        "consistency_build" => "progress",
        "consistency_maintain" => "progress",
        "streak_keepalive" => "progress",
        _ => "calendar"
    };

    internal static string RecommendationHrefWithContext(string? code, Guid? planId, DateOnly? weekStart)
    {
        var normalizedCode = NormalizeCode(code);

        return normalizedCode switch
        {
            "consistency_recovery" or "consistency_build" or "consistency_maintain" or "streak_keepalive"
                when planId.HasValue && weekStart.HasValue
                => $"progress?planId={planId.Value}&weekStart={weekStart.Value:yyyy-MM-dd}",
            _ => RecommendationHref(normalizedCode)
        };
    }

    internal static string RecommendationCtaLabel(string? code) => NormalizeCode(code) switch
    {
        "activate_plan" => "Open plans",
        "plan_next_session" => "Adjust plan",
        "next_session_gap" => "Review sessions",
        "consistency_recovery" => "Log progress",
        "consistency_build" => "Open progress",
        "consistency_maintain" => "Track this week",
        "streak_keepalive" => "Keep streak",
        _ => "Open calendar"
    };

    internal static string TrendWindowLabel(int weeksEvaluated) => weeksEvaluated <= 1
        ? "latest 1-week trend"
        : $"latest {weeksEvaluated}-week trend";

    internal static string ComplianceBandClass(decimal? compliancePercent)
    {
        if (!compliancePercent.HasValue)
        {
            return "tt-variance-neutral";
        }

        return compliancePercent.Value switch
        {
            >= 90 => "tt-compliance-good",
            >= 70 => "tt-compliance-ok",
            _ => "tt-compliance-low"
        };
    }

    internal static string ComplianceBandLabel(decimal? compliancePercent)
    {
        if (!compliancePercent.HasValue)
        {
            return "No data";
        }

        return compliancePercent.Value switch
        {
            >= 90 => "On track",
            >= 70 => "Needs a push",
            _ => "Needs attention"
        };
    }

    private static string NormalizeSeverity(string? severity) =>
        string.IsNullOrWhiteSpace(severity)
            ? "low"
            : severity.Trim().ToLowerInvariant();

    private static string NormalizeCode(string? code) =>
        string.IsNullOrWhiteSpace(code)
            ? string.Empty
            : code.Trim().ToLowerInvariant();
}

namespace TriTrainer.Web.Components.Pages;

internal static class RecordsWorkflowHelpers
{
    internal static PersonalRecordMetric DefaultMetricForDiscipline(ActivityType discipline) => discipline switch
    {
        ActivityType.Run => PersonalRecordMetric.Fastest5k,
        ActivityType.Cycle => PersonalRecordMetric.LongestRide,
        ActivityType.Swim => PersonalRecordMetric.LongestSwim,
        _ => PersonalRecordMetric.Fastest5k
    };

    internal static DateTime QuickDatePreset(string preset, DateTime today) => preset switch
    {
        "today" => today.Date,
        "yesterday" => today.Date.AddDays(-1),
        _ => today.Date
    };
}
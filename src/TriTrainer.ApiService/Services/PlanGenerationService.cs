using TriTrainer.ApiService.Data;

namespace TriTrainer.ApiService.Services;

/// <summary>
/// Generates planned sessions for a training plan using discipline-specific templates
/// and a 4-week mesocycle loading pattern.
///
/// Mesocycle pattern (by (weekIndex-1) mod 4):
///   0 = base build  (100% volume, Endurance)
///   1 = build       (105% volume, Endurance)
///   2 = peak build  (110% volume, Tempo/Intervals)
///   3 = recovery    ( 70% volume, Recovery)
/// </summary>
public static class PlanGenerationService
{
    // EventFinish template: triathlon balance (Run 35%, Cycle 40%, Swim 25%)
    private static readonly (ActivityType Discipline, DayOfWeek Day, decimal Share)[] EventFinishTemplate =
    [
        (ActivityType.Run,   DayOfWeek.Monday,    0.35m),
        (ActivityType.Cycle, DayOfWeek.Wednesday, 0.40m),
        (ActivityType.Swim,  DayOfWeek.Friday,    0.25m),
    ];

    // Consistency template: equal split across all three disciplines
    private static readonly (ActivityType Discipline, DayOfWeek Day, decimal Share)[] ConsistencyTemplate =
    [
        (ActivityType.Run,   DayOfWeek.Monday,    1m / 3m),
        (ActivityType.Cycle, DayOfWeek.Wednesday, 1m / 3m),
        (ActivityType.Swim,  DayOfWeek.Friday,    1m / 3m),
    ];

    public static IReadOnlyList<PlannedSession> GenerateSessions(
        TrainingPlan plan,
        Goal? goal,
        AthleteProfile athlete)
    {
        var sessions = new List<PlannedSession>();
        var baseMinutes = (int)(athlete.WeeklyHoursAvailable * 60);

        foreach (var week in plan.Weeks.OrderBy(w => w.WeekIndex))
        {
            var phase = (week.WeekIndex - 1) % 4;
            var loadFactor = phase switch
            {
                0 => 1.00m,
                1 => 1.05m,
                2 => 1.10m,
                3 => 0.70m,
                _ => 1.00m
            };

            var weeklyLoad = (int)(baseMinutes * loadFactor);
            var isRecovery = phase == 3;
            var isPeak = phase == 2;

            var weekSessions = goal?.GoalType switch
            {
                GoalType.DisciplinePerformance =>
                    BuildDisciplineSessions(week, goal.Discipline ?? ActivityType.Run, weeklyLoad, isRecovery, isPeak),
                GoalType.Consistency =>
                    BuildTemplateSessions(week, ConsistencyTemplate, weeklyLoad, isRecovery, isPeak),
                _ =>
                    BuildTemplateSessions(week, EventFinishTemplate, weeklyLoad, isRecovery, isPeak),
            };

            sessions.AddRange(weekSessions);
        }

        return sessions.AsReadOnly();
    }

    private static IEnumerable<PlannedSession> BuildTemplateSessions(
        PlanWeek week,
        (ActivityType Discipline, DayOfWeek Day, decimal Share)[] template,
        int weeklyLoad,
        bool isRecovery,
        bool isPeak)
    {
        foreach (var (discipline, day, share) in template)
        {
            var duration = RoundToFive(Math.Max(15, (int)(weeklyLoad * share)));
            var sessionType = isRecovery ? SessionType.Recovery
                : isPeak ? SessionType.Tempo
                : SessionType.Endurance;

            yield return new PlannedSession
            {
                PlanWeekId = week.Id,
                Discipline = discipline,
                SessionType = sessionType,
                PlannedDurationMinutes = duration,
                DayOfWeek = day
            };
        }
    }

    private static IEnumerable<PlannedSession> BuildDisciplineSessions(
        PlanWeek week,
        ActivityType focus,
        int weeklyLoad,
        bool isRecovery,
        bool isPeak)
    {
        var crossTrain = focus switch
        {
            ActivityType.Run   => ActivityType.Swim,
            ActivityType.Cycle => ActivityType.Run,
            ActivityType.Swim  => ActivityType.Cycle,
            _                  => ActivityType.Run
        };

        var primaryDur   = RoundToFive(Math.Max(15, (int)(weeklyLoad * 0.50m)));
        var secondaryDur = RoundToFive(Math.Max(15, (int)(weeklyLoad * 0.20m)));
        var crossDur     = RoundToFive(Math.Max(15, (int)(weeklyLoad * 0.30m)));

        var primaryType   = isRecovery ? SessionType.Recovery : isPeak ? SessionType.Intervals : SessionType.Endurance;
        var secondaryType = isRecovery ? SessionType.Recovery : SessionType.Tempo;
        var crossType     = isRecovery ? SessionType.Recovery : SessionType.Endurance;

        yield return new PlannedSession { PlanWeekId = week.Id, Discipline = focus,      SessionType = primaryType,   PlannedDurationMinutes = primaryDur,   DayOfWeek = DayOfWeek.Monday    };
        yield return new PlannedSession { PlanWeekId = week.Id, Discipline = focus,      SessionType = secondaryType, PlannedDurationMinutes = secondaryDur, DayOfWeek = DayOfWeek.Wednesday };
        yield return new PlannedSession { PlanWeekId = week.Id, Discipline = crossTrain, SessionType = crossType,     PlannedDurationMinutes = crossDur,     DayOfWeek = DayOfWeek.Friday    };
    }

    private static int RoundToFive(int minutes) => (minutes / 5) * 5;
}

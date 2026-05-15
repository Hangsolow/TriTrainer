namespace TriTrainer.ApiService.Data;

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

public enum SessionType
{
    Endurance,
    Tempo,
    Intervals,
    Recovery
}

public enum PersonalRecordMetric
{
    Fastest5k,
    Fastest10k,
    LongestRide,
    LongestSwim
}

public class AthleteProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;
    public decimal WeeklyHoursAvailable { get; set; }
    public DateOnly? PrimaryEventDate { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public List<Goal> Goals { get; set; } = [];
    public List<TrainingPlan> TrainingPlans { get; set; } = [];
    public List<PersonalRecord> PersonalRecords { get; set; } = [];
}

public class Goal
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid AthleteId { get; set; }
    public GoalType GoalType { get; set; }
    public ActivityType? Discipline { get; set; }
    public decimal? TargetValue { get; set; }
    public DateOnly TargetDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.Draft;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public AthleteProfile? Athlete { get; set; }
    public List<TrainingPlan> TrainingPlans { get; set; } = [];
}

public class TrainingPlan
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid AthleteId { get; set; }
    public Guid? GoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public PlanStatus Status { get; set; } = PlanStatus.Draft;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public AthleteProfile? Athlete { get; set; }
    public Goal? Goal { get; set; }
    public List<PlanWeek> Weeks { get; set; } = [];
}

public class PlanWeek
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public int WeekIndex { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public string? Notes { get; set; }

    public TrainingPlan? Plan { get; set; }
    public List<PlannedSession> Sessions { get; set; } = [];
}

public class PlannedSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PlanWeekId { get; set; }
    public ActivityType Discipline { get; set; }
    public SessionType SessionType { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public decimal? PlannedDistanceKm { get; set; }
    public DayOfWeek DayOfWeek { get; set; }

    public PlanWeek? PlanWeek { get; set; }
}

public class PersonalRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid AthleteId { get; set; }
    public ActivityType Discipline { get; set; }
    public PersonalRecordMetric Metric { get; set; }
    public decimal Value { get; set; }
    public DateOnly AchievedOn { get; set; }
    public Guid? SourceActivityId { get; set; }

    public AthleteProfile? Athlete { get; set; }
    public Activity? SourceActivity { get; set; }
}

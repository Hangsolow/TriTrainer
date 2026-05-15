using Microsoft.EntityFrameworkCore;

namespace TriTrainer.ApiService.Data;

public class ActivitiesDbContext(DbContextOptions<ActivitiesDbContext> options) : DbContext(options)
{
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<AthleteProfile> AthleteProfiles => Set<AthleteProfile>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<TrainingPlan> TrainingPlans => Set<TrainingPlan>();
    public DbSet<PlanWeek> PlanWeeks => Set<PlanWeek>();
    public DbSet<PlannedSession> PlannedSessions => Set<PlannedSession>();
    public DbSet<PersonalRecord> PersonalRecords => Set<PersonalRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Type).HasConversion<string>();
        });

        modelBuilder.Entity<AthleteProfile>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.DisplayName).HasMaxLength(120);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.GoalType).HasConversion<string>();
            entity.Property(g => g.Discipline).HasConversion<string>();
            entity.Property(g => g.Status).HasConversion<string>();

            entity.HasOne(g => g.Athlete)
                .WithMany(a => a.Goals)
                .HasForeignKey(g => g.AthleteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(120);
            entity.Property(p => p.Status).HasConversion<string>();

            entity.HasOne(p => p.Athlete)
                .WithMany(a => a.TrainingPlans)
                .HasForeignKey(p => p.AthleteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Goal)
                .WithMany(g => g.TrainingPlans)
                .HasForeignKey(p => p.GoalId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PlanWeek>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Notes).HasMaxLength(500);

            entity.HasOne(w => w.Plan)
                .WithMany(p => p.Weeks)
                .HasForeignKey(w => w.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlannedSession>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Discipline).HasConversion<string>();
            entity.Property(s => s.SessionType).HasConversion<string>();

            entity.HasOne(s => s.PlanWeek)
                .WithMany(w => w.Sessions)
                .HasForeignKey(s => s.PlanWeekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonalRecord>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Discipline).HasConversion<string>();
            entity.Property(r => r.Metric).HasConversion<string>();

            entity.HasOne(r => r.Athlete)
                .WithMany(a => a.PersonalRecords)
                .HasForeignKey(r => r.AthleteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.SourceActivity)
                .WithMany()
                .HasForeignKey(r => r.SourceActivityId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

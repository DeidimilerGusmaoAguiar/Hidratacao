namespace Hidratacao.Domain;

public sealed class Settings
{
    public Settings(
        int dailyGoalMl,
        TimeOnly activeHoursStart,
        TimeOnly activeHoursEnd,
        int reminderIntervalMinutes,
        int defaultCupMl,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        DailyGoalMl = dailyGoalMl;
        ActiveHoursStart = activeHoursStart;
        ActiveHoursEnd = activeHoursEnd;
        ReminderIntervalMinutes = reminderIntervalMinutes;
        DefaultCupMl = defaultCupMl;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public int DailyGoalMl { get; }
    public TimeOnly ActiveHoursStart { get; }
    public TimeOnly ActiveHoursEnd { get; }
    public int ReminderIntervalMinutes { get; }
    public int DefaultCupMl { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; }

    public Settings WithUpdated(
        int dailyGoalMl,
        TimeOnly activeHoursStart,
        TimeOnly activeHoursEnd,
        int reminderIntervalMinutes,
        int defaultCupMl,
        DateTimeOffset updatedAt)
    {
        return new Settings(
            dailyGoalMl,
            activeHoursStart,
            activeHoursEnd,
            reminderIntervalMinutes,
            defaultCupMl,
            CreatedAt,
            updatedAt);
    }
}

using Hidratacao.Domain;

namespace Hidratacao.Application;

public sealed record SettingsUpdate(
    int? DailyGoalMl = null,
    TimeOnly? ActiveHoursStart = null,
    TimeOnly? ActiveHoursEnd = null,
    int? ReminderIntervalMinutes = null,
    int? DefaultCupMl = null
);

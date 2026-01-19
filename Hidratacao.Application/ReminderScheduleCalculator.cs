namespace Hidratacao.Application;

public static class ReminderScheduleCalculator
{
    public static DateTime GetNextReminderAtLocal(
        Hidratacao.Domain.Settings settings,
        DateTime nowLocal,
        DateTime? lastEventLocal = null)
    {
        var start = DateTime.Today.Add(settings.ActiveHoursStart.ToTimeSpan());
        var end = DateTime.Today.Add(settings.ActiveHoursEnd.ToTimeSpan());

        if (nowLocal < start)
        {
            return start;
        }

        if (nowLocal >= end)
        {
            return start.AddDays(1);
        }

        var threshold = nowLocal.AddMinutes(-settings.ReminderIntervalMinutes);
        var seed = lastEventLocal.HasValue && lastEventLocal.Value > threshold
            ? lastEventLocal.Value
            : nowLocal;

        var next = seed.AddMinutes(settings.ReminderIntervalMinutes);
        if (next > end)
        {
            return start.AddDays(1);
        }

        return next;
    }
}

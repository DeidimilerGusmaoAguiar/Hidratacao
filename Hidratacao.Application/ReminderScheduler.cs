namespace Hidratacao.Application;

public sealed class ReminderScheduler
{
    private readonly SettingsService _settingsService;
    private readonly WaterHistoryService _historyService;

    public ReminderScheduler(SettingsService settingsService, WaterHistoryService historyService)
    {
        _settingsService = settingsService;
        _historyService = historyService;
    }

    public event EventHandler<ReminderEventArgs>? Reminder;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var settings = await _settingsService.GetAsync(cancellationToken);
            var now = DateTime.Now;
            var start = DateTime.Today.Add(settings.ActiveHoursStart.ToTimeSpan());
            var end = DateTime.Today.Add(settings.ActiveHoursEnd.ToTimeSpan());

            if (now < start)
            {
                await DelaySafe(start - now, cancellationToken);
                continue;
            }

            if (now >= end)
            {
                await DelaySafe(start.AddDays(1) - now, cancellationToken);
                continue;
            }

            var next = now.AddMinutes(settings.ReminderIntervalMinutes);
            if (next > end)
            {
                await DelaySafe(start.AddDays(1) - now, cancellationToken);
                continue;
            }

            await DelaySafe(next - now, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await EmitReminderAsync(settings);
        }
    }

    private async Task EmitReminderAsync(Hidratacao.Domain.Settings settings)
    {
        var history = await _historyService.GetHistoryAsync(1);
        var total = history.Count == 0 ? 0 : history[0].TotalMl;
        var remaining = Math.Max(settings.DailyGoalMl - total, 0);

        Reminder?.Invoke(this, new ReminderEventArgs(DateTime.Now, remaining, settings.DefaultCupMl));
    }

    private static Task DelaySafe(TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        return Task.Delay(delay, cancellationToken);
    }
}

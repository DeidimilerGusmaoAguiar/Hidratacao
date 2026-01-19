using Hidratacao.Domain;

namespace Hidratacao.Application;

public sealed class WaterHistoryService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IDailySummaryRepository _summaryRepository;
    private readonly IWaterEventRepository _eventRepository;

    public WaterHistoryService(
        ISettingsRepository settingsRepository,
        IDailySummaryRepository summaryRepository,
        IWaterEventRepository eventRepository)
    {
        _settingsRepository = settingsRepository;
        _summaryRepository = summaryRepository;
        _eventRepository = eventRepository;
    }

    public async Task<IReadOnlyList<WaterHistoryItem>> GetHistoryAsync(
        int days,
        CancellationToken cancellationToken = default)
    {
        if (days <= 0)
        {
            return Array.Empty<WaterHistoryItem>();
        }

        var settings = await _settingsRepository.GetAsync(cancellationToken);
        var dailyGoal = settings.DailyGoalMl;

        var summaries = await _summaryRepository.GetAllAsync(cancellationToken);
        var events = await _eventRepository.GetAllAsync(cancellationToken);

        var totalsByDate = new Dictionary<DateOnly, int>();

        foreach (var summary in summaries)
        {
            totalsByDate[summary.DateUtc] = summary.TotalMl;
        }

        if (events.Count > 0)
        {
            totalsByDate.Clear();
            foreach (var waterEvent in events)
            {
                var date = DateOnly.FromDateTime(waterEvent.OccurredAtUtc.UtcDateTime);
                totalsByDate.TryGetValue(date, out var total);
                totalsByDate[date] = total + waterEvent.AmountMl;
            }
        }

        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = new List<WaterHistoryItem>(days);

        for (var offset = 0; offset < days; offset++)
        {
            var date = todayUtc.AddDays(-offset);
            totalsByDate.TryGetValue(date, out var total);
            items.Add(new WaterHistoryItem(date, total, dailyGoal, date == todayUtc));
        }

        return items;
    }

    public async Task<DateTime?> GetLastEventLocalAsync(CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetAllAsync(cancellationToken);
        if (events.Count == 0)
        {
            return null;
        }

        var last = events.Max(e => e.OccurredAtUtc.UtcDateTime);
        return DateTime.SpecifyKind(last, DateTimeKind.Utc).ToLocalTime();
    }
}

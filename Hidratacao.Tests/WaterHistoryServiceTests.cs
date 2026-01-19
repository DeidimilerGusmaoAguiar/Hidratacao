using Hidratacao.Application;
using Hidratacao.Domain;
using Hidratacao.Infrastructure;

namespace Hidratacao.Tests;

public class WaterHistoryServiceTests : IDisposable
{
    private readonly string _basePath;
    private readonly JsonSettingsRepository _settingsRepository;
    private readonly JsonDailySummaryRepository _summaryRepository;
    private readonly JsonWaterEventRepository _eventRepository;
    private readonly WaterHistoryService _service;

    public WaterHistoryServiceTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "hidratacao-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_basePath);
        _settingsRepository = new JsonSettingsRepository(_basePath);
        _summaryRepository = new JsonDailySummaryRepository(_basePath);
        _eventRepository = new JsonWaterEventRepository(_basePath);
        _service = new WaterHistoryService(_settingsRepository, _summaryRepository, _eventRepository);

        var now = DateTimeOffset.UtcNow;
        _settingsRepository.SaveAsync(new Settings(
            dailyGoalMl: 2000,
            activeHoursStart: new TimeOnly(8, 0),
            activeHoursEnd: new TimeOnly(22, 0),
            reminderIntervalMinutes: 30,
            defaultCupMl: 250,
            createdAt: now,
            updatedAt: now)).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetHistoryAsync_UsesEventsWhenPresent()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var events = new List<WaterEvent>
        {
            new WaterEvent(Guid.NewGuid(), 200, new DateTimeOffset(today.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero)),
            new WaterEvent(Guid.NewGuid(), 300, new DateTimeOffset(today.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero)),
            new WaterEvent(Guid.NewGuid(), 150, new DateTimeOffset(yesterday.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero))
        };

        await _eventRepository.SaveAllAsync(events);

        var history = await _service.GetHistoryAsync(2);

        Assert.Equal(2, history.Count);
        Assert.Equal(500, history[0].TotalMl);
        Assert.Equal(150, history[1].TotalMl);
    }

    [Fact]
    public async Task GetHistoryAsync_UsesSummariesWhenNoEvents()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var summaries = new List<DailySummary>
        {
            new DailySummary(today, 700, DateTimeOffset.UtcNow)
        };

        await _summaryRepository.SaveAllAsync(summaries);

        var history = await _service.GetHistoryAsync(1);

        Assert.Single(history);
        Assert.Equal(700, history[0].TotalMl);
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }
}

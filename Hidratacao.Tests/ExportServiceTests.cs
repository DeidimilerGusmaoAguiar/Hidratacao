using Hidratacao.Application;
using Hidratacao.Domain;
using Hidratacao.Infrastructure;

namespace Hidratacao.Tests;

public class ExportServiceTests : IDisposable
{
    private readonly string _basePath;
    private readonly string _exportPath;
    private readonly JsonSettingsRepository _settingsRepository;
    private readonly JsonDailySummaryRepository _summaryRepository;
    private readonly JsonWaterEventRepository _eventRepository;
    private readonly WaterHistoryService _historyService;
    private readonly ExportService _exportService;

    public ExportServiceTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "hidratacao-tests", Guid.NewGuid().ToString("N"));
        _exportPath = Path.Combine(_basePath, "exports");
        Directory.CreateDirectory(_basePath);

        _settingsRepository = new JsonSettingsRepository(_basePath);
        _summaryRepository = new JsonDailySummaryRepository(_basePath);
        _eventRepository = new JsonWaterEventRepository(_basePath);
        _historyService = new WaterHistoryService(_settingsRepository, _summaryRepository, _eventRepository);
        _exportService = new ExportService(_historyService, _eventRepository);

        var now = DateTimeOffset.UtcNow;
        _settingsRepository.SaveAsync(new Settings(
            dailyGoalMl: 1800,
            activeHoursStart: new TimeOnly(8, 0),
            activeHoursEnd: new TimeOnly(22, 0),
            reminderIntervalMinutes: 30,
            defaultCupMl: 250,
            createdAt: now,
            updatedAt: now)).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ExportAsync_WritesTotalsAndEvents()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldDate = today.AddDays(-10);

        var events = new List<WaterEvent>
        {
            new WaterEvent(Guid.NewGuid(), 250, new DateTimeOffset(today.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero)),
            new WaterEvent(Guid.NewGuid(), 300, new DateTimeOffset(oldDate.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero))
        };

        await _eventRepository.SaveAllAsync(events);

        var result = await _exportService.ExportAsync(_exportPath, 2);

        Assert.True(File.Exists(result.TotalsPath));
        Assert.True(File.Exists(result.EventsPath));

        var totals = await File.ReadAllTextAsync(result.TotalsPath);
        Assert.Contains("date_utc,total_ml,daily_goal_ml,status,progress_percent", totals);

        var eventsCsv = await File.ReadAllTextAsync(result.EventsPath);
        Assert.Contains("id,occurred_at_utc,amount_ml", eventsCsv);
        Assert.DoesNotContain(oldDate.ToString("yyyy-MM-dd"), eventsCsv);
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }
}

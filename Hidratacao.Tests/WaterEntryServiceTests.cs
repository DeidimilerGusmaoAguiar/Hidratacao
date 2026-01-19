using Hidratacao.Application;
using Hidratacao.Domain;
using Hidratacao.Infrastructure;

namespace Hidratacao.Tests;

public class WaterEntryServiceTests : IDisposable
{
    private readonly string _basePath;
    private readonly JsonWaterEventRepository _eventRepository;
    private readonly JsonDailySummaryRepository _summaryRepository;
    private readonly WaterEntryService _service;

    public WaterEntryServiceTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "hidratacao-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_basePath);
        _eventRepository = new JsonWaterEventRepository(_basePath);
        _summaryRepository = new JsonDailySummaryRepository(_basePath);
        _service = new WaterEntryService(_eventRepository, _summaryRepository);
    }

    [Fact]
    public async Task AddMlAsync_UpdatesTotal()
    {
        var result = await _service.AddMlAsync(250);

        Assert.True(result.Success);
        Assert.Equal(250, result.TotalTodayMl);
    }

    [Fact]
    public async Task UndoLastTodayAsync_RemovesLast()
    {
        await _service.AddMlAsync(200);
        await _service.AddMlAsync(300);

        var result = await _service.UndoLastTodayAsync();

        Assert.True(result.Success);
        Assert.Equal(200, result.TotalTodayMl);
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }
}

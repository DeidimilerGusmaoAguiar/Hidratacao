using System.Globalization;

namespace Hidratacao.Application;

public sealed class ExportService
{
    private readonly WaterHistoryService _historyService;
    private readonly IWaterEventRepository _eventRepository;

    public ExportService(WaterHistoryService historyService, IWaterEventRepository eventRepository)
    {
        _historyService = historyService;
        _eventRepository = eventRepository;
    }

    public async Task<ExportResult> ExportAsync(
        string directory,
        int days,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = Environment.CurrentDirectory;
        }

        Directory.CreateDirectory(directory);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var totalsPath = Path.Combine(directory, $"totals_{timestamp}.csv");
        var eventsPath = Path.Combine(directory, $"events_{timestamp}.csv");

        var history = await _historyService.GetHistoryAsync(days, cancellationToken);
        var totalsCsv = CsvExportFormatter.FormatTotals(history);
        await File.WriteAllTextAsync(totalsPath, totalsCsv, cancellationToken);

        var events = await _eventRepository.GetAllAsync(cancellationToken);
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(days - 1));
        var filtered = events
            .Where(e => DateOnly.FromDateTime(e.OccurredAtUtc.UtcDateTime) >= cutoff)
            .ToList();

        var eventsCsv = CsvExportFormatter.FormatEvents(filtered);
        await File.WriteAllTextAsync(eventsPath, eventsCsv, cancellationToken);

        return new ExportResult(totalsPath, eventsPath);
    }
}

using Hidratacao.Domain;

namespace Hidratacao.Application;

public sealed class WaterEntryService
{
    private readonly IWaterEventRepository _eventRepository;
    private readonly IDailySummaryRepository _summaryRepository;

    public WaterEntryService(IWaterEventRepository eventRepository, IDailySummaryRepository summaryRepository)
    {
        _eventRepository = eventRepository;
        _summaryRepository = summaryRepository;
    }

    public Task<WaterEntryResult> AddMlAsync(int amountMl, CancellationToken cancellationToken = default)
    {
        if (amountMl <= 0)
        {
            return Task.FromResult(WaterEntryResult.Fail("Quantidade deve ser maior que zero."));
        }

        return AddEventAsync(amountMl, cancellationToken);
    }

    public Task<WaterEntryResult> AddCupAsync(int cupMl, CancellationToken cancellationToken = default)
    {
        if (cupMl <= 0)
        {
            return Task.FromResult(WaterEntryResult.Fail("Copo padrão inválido."));
        }

        return AddEventAsync(cupMl, cancellationToken);
    }

    public async Task<WaterEntryResult> UndoLastTodayAsync(CancellationToken cancellationToken = default)
    {
        var events = (await _eventRepository.GetAllAsync(cancellationToken)).ToList();
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);

        var last = events
            .Where(e => DateOnly.FromDateTime(e.OccurredAtUtc.UtcDateTime) == todayUtc)
            .OrderByDescending(e => e.OccurredAtUtc)
            .FirstOrDefault();

        if (last is null)
        {
            return WaterEntryResult.Fail("Nenhum registro encontrado para hoje.");
        }

        events.Remove(last);
        await _eventRepository.SaveAllAsync(events, cancellationToken);

        var total = await UpdateDailySummaryAsync(events, todayUtc, cancellationToken);
        return WaterEntryResult.Ok(total);
    }

    private async Task<WaterEntryResult> AddEventAsync(int amountMl, CancellationToken cancellationToken)
    {
        var events = (await _eventRepository.GetAllAsync(cancellationToken)).ToList();
        var now = DateTimeOffset.UtcNow;
        var todayUtc = DateOnly.FromDateTime(now.UtcDateTime);

        events.Add(new WaterEvent(Guid.NewGuid(), amountMl, now));
        await _eventRepository.SaveAllAsync(events, cancellationToken);

        var total = await UpdateDailySummaryAsync(events, todayUtc, cancellationToken);
        return WaterEntryResult.Ok(total);
    }

    private async Task<int> UpdateDailySummaryAsync(
        IReadOnlyList<WaterEvent> events,
        DateOnly dateUtc,
        CancellationToken cancellationToken)
    {
        var total = events
            .Where(e => DateOnly.FromDateTime(e.OccurredAtUtc.UtcDateTime) == dateUtc)
            .Sum(e => e.AmountMl);

        var summaries = (await _summaryRepository.GetAllAsync(cancellationToken)).ToList();
        var existing = summaries.FirstOrDefault(s => s.DateUtc == dateUtc);
        var updatedAt = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            summaries.Add(new DailySummary(dateUtc, total, updatedAt));
        }
        else
        {
            summaries.Remove(existing);
            summaries.Add(new DailySummary(dateUtc, total, updatedAt));
        }

        await _summaryRepository.SaveAllAsync(summaries, cancellationToken);
        return total;
    }
}

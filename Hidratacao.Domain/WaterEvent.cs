namespace Hidratacao.Domain;

public sealed class WaterEvent
{
    public WaterEvent(Guid id, int amountMl, DateTimeOffset occurredAtUtc)
    {
        Id = id;
        AmountMl = amountMl;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }
    public int AmountMl { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}

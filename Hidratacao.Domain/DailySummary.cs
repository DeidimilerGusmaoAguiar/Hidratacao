namespace Hidratacao.Domain;

public sealed class DailySummary
{
    public DailySummary(DateOnly dateUtc, int totalMl, DateTimeOffset updatedAtUtc)
    {
        DateUtc = dateUtc;
        TotalMl = totalMl;
        UpdatedAtUtc = updatedAtUtc;
    }

    public DateOnly DateUtc { get; }
    public int TotalMl { get; }
    public DateTimeOffset UpdatedAtUtc { get; }
}

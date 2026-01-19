namespace Hidratacao.Application;

public sealed class WaterEntryResult
{
    private WaterEntryResult(bool success, int totalTodayMl, string? error)
    {
        Success = success;
        TotalTodayMl = totalTodayMl;
        Error = error;
    }

    public bool Success { get; }
    public int TotalTodayMl { get; }
    public string? Error { get; }

    public static WaterEntryResult Ok(int totalTodayMl)
        => new(true, totalTodayMl, null);

    public static WaterEntryResult Fail(string error)
        => new(false, 0, error);
}

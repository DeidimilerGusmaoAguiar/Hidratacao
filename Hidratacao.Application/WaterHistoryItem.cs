namespace Hidratacao.Application;

public sealed class WaterHistoryItem
{
    public WaterHistoryItem(DateOnly dateUtc, int totalMl, int dailyGoalMl, bool isCurrentDay)
    {
        DateUtc = dateUtc;
        TotalMl = totalMl;
        DailyGoalMl = dailyGoalMl;
        IsCurrentDay = isCurrentDay;
    }

    public DateOnly DateUtc { get; }
    public int TotalMl { get; }
    public int DailyGoalMl { get; }
    public bool IsCurrentDay { get; }

    public string Status => TotalMl >= DailyGoalMl ? "completo" : "incompleto";

    public int ProgressPercent
    {
        get
        {
            if (DailyGoalMl <= 0)
            {
                return 0;
            }

            var percent = (double)TotalMl / DailyGoalMl * 100;
            return (int)Math.Round(percent, MidpointRounding.AwayFromZero);
        }
    }
}

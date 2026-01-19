using System.Globalization;
using System.Text;
using Hidratacao.Domain;

namespace Hidratacao.Application;

public static class CsvExportFormatter
{
    public static string FormatTotals(IReadOnlyList<WaterHistoryItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine("date_utc,total_ml,daily_goal_ml,status,progress_percent");

        foreach (var item in items.OrderBy(i => i.DateUtc))
        {
            var fields = new[]
            {
                item.DateUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                item.TotalMl.ToString(CultureInfo.InvariantCulture),
                item.DailyGoalMl.ToString(CultureInfo.InvariantCulture),
                item.Status,
                item.ProgressPercent.ToString(CultureInfo.InvariantCulture)
            };
            builder.AppendLine(JoinCsv(fields));
        }

        return builder.ToString();
    }

    public static string FormatEvents(IReadOnlyList<WaterEvent> events)
    {
        var builder = new StringBuilder();
        builder.AppendLine("id,occurred_at_utc,amount_ml");

        foreach (var waterEvent in events.OrderBy(e => e.OccurredAtUtc))
        {
            var fields = new[]
            {
                waterEvent.Id.ToString(),
                waterEvent.OccurredAtUtc.ToString("O", CultureInfo.InvariantCulture),
                waterEvent.AmountMl.ToString(CultureInfo.InvariantCulture)
            };
            builder.AppendLine(JoinCsv(fields));
        }

        return builder.ToString();
    }

    private static string JoinCsv(IEnumerable<string> fields)
    {
        return string.Join(",", fields.Select(Escape));
    }

    private static string Escape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

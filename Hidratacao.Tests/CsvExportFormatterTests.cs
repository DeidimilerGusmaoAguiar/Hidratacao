using Hidratacao.Application;
using Hidratacao.Domain;

namespace Hidratacao.Tests;

public class CsvExportFormatterTests
{
    [Fact]
    public void FormatEvents_EscapesFields()
    {
        var events = new List<WaterEvent>
        {
            new WaterEvent(Guid.NewGuid(), 250, DateTimeOffset.Parse("2026-01-19T10:00:00Z")),
            new WaterEvent(Guid.NewGuid(), 300, DateTimeOffset.Parse("2026-01-19T10:30:00Z"))
        };

        var csv = CsvExportFormatter.FormatEvents(events);

        Assert.Contains("id,occurred_at_utc,amount_ml", csv);
        Assert.Contains("2026-01-19T10:00:00.0000000+00:00", csv);
    }
}

namespace Hidratacao.Application;

public sealed class ExportResult
{
    public ExportResult(string totalsPath, string eventsPath)
    {
        TotalsPath = totalsPath;
        EventsPath = eventsPath;
    }

    public string TotalsPath { get; }
    public string EventsPath { get; }
}

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hidratacao.Application;
using Hidratacao.Domain;

namespace Hidratacao.Infrastructure;

public sealed class JsonDailySummaryRepository : IDailySummaryRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonDailySummaryRepository(string basePath)
    {
        _filePath = Path.Combine(basePath, "daily_summary.json");
        _options = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            WriteIndented = true
        };
    }

    public async Task<IReadOnlyList<DailySummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            await SaveAllAsync(Array.Empty<DailySummary>(), cancellationToken);
            return Array.Empty<DailySummary>();
        }

        await using var stream = File.OpenRead(_filePath);
        var models = await JsonSerializer.DeserializeAsync<List<DailySummaryJson>>(stream, _options, cancellationToken);
        if (models is null)
        {
            return Array.Empty<DailySummary>();
        }

        return models.Select(m => m.ToDomain()).ToList();
    }

    public async Task SaveAllAsync(IReadOnlyList<DailySummary> summaries, CancellationToken cancellationToken = default)
    {
        EnsureDirectory();
        var models = summaries.Select(DailySummaryJson.FromDomain).ToList();

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, models, _options, cancellationToken);
    }

    private void EnsureDirectory()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private sealed class DailySummaryJson
    {
        [JsonPropertyName("date_utc")]
        public string DateUtc { get; set; } = string.Empty;

        [JsonPropertyName("total_ml")]
        public int TotalMl { get; set; }

        [JsonPropertyName("updated_at_utc")]
        public string UpdatedAtUtc { get; set; } = string.Empty;

        public DailySummary ToDomain()
        {
            return new DailySummary(
                DateOnly.ParseExact(DateUtc, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                TotalMl,
                DateTimeOffset.Parse(UpdatedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        }

        public static DailySummaryJson FromDomain(DailySummary summary)
        {
            return new DailySummaryJson
            {
                DateUtc = summary.DateUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                TotalMl = summary.TotalMl,
                UpdatedAtUtc = summary.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture)
            };
        }
    }
}

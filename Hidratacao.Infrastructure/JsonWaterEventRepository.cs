using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hidratacao.Application;
using Hidratacao.Domain;

namespace Hidratacao.Infrastructure;

public sealed class JsonWaterEventRepository : IWaterEventRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonWaterEventRepository(string basePath)
    {
        _filePath = Path.Combine(basePath, "water_events.json");
        _options = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            WriteIndented = true
        };
    }

    public async Task<IReadOnlyList<WaterEvent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            await SaveAllAsync(Array.Empty<WaterEvent>(), cancellationToken);
            return Array.Empty<WaterEvent>();
        }

        await using var stream = File.OpenRead(_filePath);
        var models = await JsonSerializer.DeserializeAsync<List<WaterEventJson>>(stream, _options, cancellationToken);
        if (models is null)
        {
            return Array.Empty<WaterEvent>();
        }

        return models.Select(m => m.ToDomain()).ToList();
    }

    public async Task SaveAllAsync(IReadOnlyList<WaterEvent> events, CancellationToken cancellationToken = default)
    {
        EnsureDirectory();
        var models = events.Select(WaterEventJson.FromDomain).ToList();

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

    private sealed class WaterEventJson
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("amount_ml")]
        public int AmountMl { get; set; }

        [JsonPropertyName("occurred_at_utc")]
        public string OccurredAtUtc { get; set; } = string.Empty;

        public WaterEvent ToDomain()
        {
            return new WaterEvent(
                Guid.Parse(Id),
                AmountMl,
                DateTimeOffset.Parse(OccurredAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        }

        public static WaterEventJson FromDomain(WaterEvent waterEvent)
        {
            return new WaterEventJson
            {
                Id = waterEvent.Id.ToString(),
                AmountMl = waterEvent.AmountMl,
                OccurredAtUtc = waterEvent.OccurredAtUtc.ToString("O", CultureInfo.InvariantCulture)
            };
        }
    }
}

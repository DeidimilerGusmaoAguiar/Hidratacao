using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hidratacao.Application;
using Hidratacao.Domain;

namespace Hidratacao.Infrastructure;

public sealed class JsonSettingsRepository : ISettingsRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonSettingsRepository(string basePath)
    {
        _filePath = Path.Combine(basePath, "settings.json");
        _options = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            WriteIndented = true
        };
    }

    public async Task<Settings> GetAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            var defaults = CreateDefaultSettings();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        await using var stream = File.OpenRead(_filePath);
        var model = await JsonSerializer.DeserializeAsync<SettingsJson>(stream, _options, cancellationToken);
        if (model is null)
        {
            throw new InvalidDataException("settings.json está vazio ou inválido.");
        }

        return model.ToDomain();
    }

    public async Task SaveAsync(Settings settings, CancellationToken cancellationToken = default)
    {
        EnsureDirectory();
        var model = SettingsJson.FromDomain(settings);

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, model, _options, cancellationToken);
    }

    private void EnsureDirectory()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static Settings CreateDefaultSettings()
    {
        var now = DateTimeOffset.UtcNow;
        return new Settings(
            dailyGoalMl: 2000,
            activeHoursStart: new TimeOnly(8, 0),
            activeHoursEnd: new TimeOnly(22, 0),
            reminderIntervalMinutes: 30,
            defaultCupMl: 250,
            createdAt: now,
            updatedAt: now);
    }

    private sealed class SettingsJson
    {
        [JsonPropertyName("daily_goal_ml")]
        public int DailyGoalMl { get; set; }

        [JsonPropertyName("active_hours_start")]
        public string ActiveHoursStart { get; set; } = string.Empty;

        [JsonPropertyName("active_hours_end")]
        public string ActiveHoursEnd { get; set; } = string.Empty;

        [JsonPropertyName("reminder_interval_minutes")]
        public int ReminderIntervalMinutes { get; set; }

        [JsonPropertyName("default_cup_ml")]
        public int DefaultCupMl { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = string.Empty;

        public Settings ToDomain()
        {
            return new Settings(
                DailyGoalMl,
                TimeOnly.ParseExact(ActiveHoursStart, "HH:mm", CultureInfo.InvariantCulture),
                TimeOnly.ParseExact(ActiveHoursEnd, "HH:mm", CultureInfo.InvariantCulture),
                ReminderIntervalMinutes,
                DefaultCupMl,
                DateTimeOffset.Parse(CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTimeOffset.Parse(UpdatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        }

        public static SettingsJson FromDomain(Settings settings)
        {
            return new SettingsJson
            {
                DailyGoalMl = settings.DailyGoalMl,
                ActiveHoursStart = settings.ActiveHoursStart.ToString("HH:mm", CultureInfo.InvariantCulture),
                ActiveHoursEnd = settings.ActiveHoursEnd.ToString("HH:mm", CultureInfo.InvariantCulture),
                ReminderIntervalMinutes = settings.ReminderIntervalMinutes,
                DefaultCupMl = settings.DefaultCupMl,
                CreatedAt = settings.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
                UpdatedAt = settings.UpdatedAt.ToString("O", CultureInfo.InvariantCulture)
            };
        }
    }
}

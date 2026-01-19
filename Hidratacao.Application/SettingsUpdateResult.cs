using Hidratacao.Domain;

namespace Hidratacao.Application;

public sealed class SettingsUpdateResult
{
    private SettingsUpdateResult(Settings? settings, IReadOnlyList<string> errors)
    {
        Settings = settings;
        Errors = errors;
    }

    public Settings? Settings { get; }
    public IReadOnlyList<string> Errors { get; }
    public bool Success => Errors.Count == 0 && Settings is not null;

    public static SettingsUpdateResult Ok(Settings settings)
        => new(settings, Array.Empty<string>());

    public static SettingsUpdateResult Fail(IReadOnlyList<string> errors)
        => new(null, errors);
}

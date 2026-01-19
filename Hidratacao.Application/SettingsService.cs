using Hidratacao.Domain;

namespace Hidratacao.Application;

public sealed class SettingsService
{
    private readonly ISettingsRepository _repository;

    public SettingsService(ISettingsRepository repository)
    {
        _repository = repository;
    }

    public Task<Settings> GetAsync(CancellationToken cancellationToken = default)
        => _repository.GetAsync(cancellationToken);

    public async Task<SettingsUpdateResult> UpdateAsync(
        SettingsUpdate update,
        CancellationToken cancellationToken = default)
    {
        var current = await _repository.GetAsync(cancellationToken);

        var next = current.WithUpdated(
            update.DailyGoalMl ?? current.DailyGoalMl,
            update.ActiveHoursStart ?? current.ActiveHoursStart,
            update.ActiveHoursEnd ?? current.ActiveHoursEnd,
            update.ReminderIntervalMinutes ?? current.ReminderIntervalMinutes,
            update.DefaultCupMl ?? current.DefaultCupMl,
            DateTimeOffset.UtcNow);

        var errors = SettingsValidator.Validate(next);
        if (errors.Count > 0)
        {
            return SettingsUpdateResult.Fail(errors);
        }

        await _repository.SaveAsync(next, cancellationToken);
        return SettingsUpdateResult.Ok(next);
    }
}

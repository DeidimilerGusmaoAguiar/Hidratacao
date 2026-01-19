using Hidratacao.Domain;

namespace Hidratacao.Tests;

public class SettingsValidatorTests
{
    [Fact]
    public void Validate_ReturnsErrors_ForInvalidValues()
    {
        var settings = new Settings(
            dailyGoalMl: 0,
            activeHoursStart: new TimeOnly(10, 0),
            activeHoursEnd: new TimeOnly(9, 0),
            reminderIntervalMinutes: -1,
            defaultCupMl: 0,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow);

        var errors = SettingsValidator.Validate(settings);

        Assert.Contains("Meta diária deve ser maior que zero.", errors);
        Assert.Contains("Intervalo de lembretes deve ser maior que zero.", errors);
        Assert.Contains("Tamanho do copo padrão deve ser maior que zero.", errors);
        Assert.Contains("Horário final deve ser maior que o horário inicial.", errors);
    }
}

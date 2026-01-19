namespace Hidratacao.Domain;

public static class SettingsValidator
{
    public static IReadOnlyList<string> Validate(Settings settings)
    {
        var errors = new List<string>();

        if (settings.DailyGoalMl <= 0)
        {
            errors.Add("Meta diária deve ser maior que zero.");
        }

        if (settings.ReminderIntervalMinutes <= 0)
        {
            errors.Add("Intervalo de lembretes deve ser maior que zero.");
        }

        if (settings.DefaultCupMl <= 0)
        {
            errors.Add("Tamanho do copo padrão deve ser maior que zero.");
        }

        if (settings.ActiveHoursEnd <= settings.ActiveHoursStart)
        {
            errors.Add("Horário final deve ser maior que o horário inicial.");
        }

        return errors;
    }
}

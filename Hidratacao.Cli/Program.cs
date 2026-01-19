using System.Globalization;
using Hidratacao.Application;
using Hidratacao.Infrastructure;

if (args.Length == 0)
{
    ShowHelp();
    return;
}

var command = args[0].ToLowerInvariant();
if (command != "config")
{
    Console.WriteLine("Comando inválido.");
    ShowHelp();
    return;
}

var options = args.Skip(1).ToArray();
if (options.Length == 0)
{
    ShowConfigHelp();
    return;
}

var basePath = Environment.CurrentDirectory;
var repository = new JsonSettingsRepository(basePath);
var service = new SettingsService(repository);

var show = false;
var update = new SettingsUpdate();

for (var i = 0; i < options.Length; i++)
{
    var option = options[i].ToLowerInvariant();
    switch (option)
    {
        case "--show":
            show = true;
            break;
        case "--goal":
            if (!TryReadInt(options, ref i, out var goal))
            {
                Console.WriteLine("Informe um valor inteiro para --goal.");
                return;
            }
            update = update with { DailyGoalMl = goal };
            break;
        case "--active-hours":
            if (!TryReadString(options, ref i, out var range) ||
                !TryParseHoursRange(range, out var start, out var end))
            {
                Console.WriteLine("Formato inválido. Use HH:MM-HH:MM em --active-hours.");
                return;
            }
            update = update with { ActiveHoursStart = start, ActiveHoursEnd = end };
            break;
        case "--interval":
            if (!TryReadInt(options, ref i, out var interval))
            {
                Console.WriteLine("Informe um valor inteiro para --interval.");
                return;
            }
            update = update with { ReminderIntervalMinutes = interval };
            break;
        case "--cup-size":
            if (!TryReadInt(options, ref i, out var cup))
            {
                Console.WriteLine("Informe um valor inteiro para --cup-size.");
                return;
            }
            update = update with { DefaultCupMl = cup };
            break;
        default:
            Console.WriteLine($"Opção desconhecida: {options[i]}");
            ShowConfigHelp();
            return;
    }
}

var hasUpdate = update != new SettingsUpdate();
if (hasUpdate)
{
    var result = await service.UpdateAsync(update);
    if (!result.Success)
    {
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Erro: {error}");
        }
        return;
    }
}

if (show || !hasUpdate)
{
    var settings = await service.GetAsync();
    PrintSettings(settings);
}

static void ShowHelp()
{
    Console.WriteLine("Uso:");
    Console.WriteLine("  hidratacao config --show");
    Console.WriteLine("  hidratacao config --goal 2000 --active-hours 08:00-22:00 --interval 30 --cup-size 250");
}

static void ShowConfigHelp()
{
    Console.WriteLine("Uso do comando config:");
    Console.WriteLine("  --show");
    Console.WriteLine("  --goal <ml>");
    Console.WriteLine("  --active-hours <HH:MM-HH:MM>");
    Console.WriteLine("  --interval <minutos>");
    Console.WriteLine("  --cup-size <ml>");
}

static bool TryReadInt(string[] options, ref int index, out int value)
{
    value = 0;
    if (index + 1 >= options.Length)
    {
        return false;
    }

    index++;
    return int.TryParse(options[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
}

static bool TryReadString(string[] options, ref int index, out string value)
{
    value = string.Empty;
    if (index + 1 >= options.Length)
    {
        return false;
    }

    index++;
    value = options[index];
    return true;
}

static bool TryParseHoursRange(string range, out TimeOnly start, out TimeOnly end)
{
    start = default;
    end = default;
    var parts = range.Split('-', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 2)
    {
        return false;
    }

    return TimeOnly.TryParseExact(parts[0], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start) &&
           TimeOnly.TryParseExact(parts[1], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out end);
}

static void PrintSettings(Hidratacao.Domain.Settings settings)
{
    Console.WriteLine("Configurações atuais:");
    Console.WriteLine($"  Meta diária (ml): {settings.DailyGoalMl}");
    Console.WriteLine($"  Horário ativo: {settings.ActiveHoursStart:HH\\:mm}-{settings.ActiveHoursEnd:HH\\:mm}");
    Console.WriteLine($"  Intervalo lembretes (min): {settings.ReminderIntervalMinutes}");
    Console.WriteLine($"  Copo padrão (ml): {settings.DefaultCupMl}");
}

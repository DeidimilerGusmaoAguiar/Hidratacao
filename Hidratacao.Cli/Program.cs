using System.Diagnostics;
using System.Globalization;
using Hidratacao.Application;
using Hidratacao.Infrastructure;

if (args.Length == 0)
{
    ShowHelp();
    return;
}

var command = args[0].ToLowerInvariant();
switch (command)
{
    case "config":
        await RunConfigAsync(args.Skip(1).ToArray());
        break;
    case "daemon":
        await RunDaemonAsync(args.Skip(1).ToArray());
        break;
    case "log":
        await RunLogAsync(args.Skip(1).ToArray());
        break;
    default:
        Console.WriteLine("Comando inválido.");
        ShowHelp();
        break;
}

static async Task RunConfigAsync(string[] options)
{
    if (options.Length == 0)
    {
        ShowConfigHelp();
        return;
    }

    var service = CreateSettingsService();
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
}

static async Task RunDaemonAsync(string[] options)
{
    if (options.Length == 0)
    {
        ShowDaemonHelp();
        return;
    }

    var action = options[0].ToLowerInvariant();
    switch (action)
    {
        case "start":
            await StartDaemonAsync();
            break;
        case "stop":
            StopDaemon();
            break;
        default:
            Console.WriteLine("Ação inválida para daemon.");
            ShowDaemonHelp();
            break;
    }
}

static async Task RunLogAsync(string[] options)
{
    if (options.Length == 0)
    {
        ShowLogHelp();
        return;
    }

    var service = CreateWaterEntryService();
    var option = options[0].ToLowerInvariant();

    if (option == "--undo")
    {
        var undo = await service.UndoLastTodayAsync();
        if (!undo.Success)
        {
            Console.WriteLine($"Erro: {undo.Error}");
            return;
        }

        Console.WriteLine($"Último registro removido. Total de hoje: {undo.TotalTodayMl} ml.");
        return;
    }

    if (option == "--cup")
    {
        var settings = await CreateSettingsService().GetAsync();
        var result = await service.AddCupAsync(settings.DefaultCupMl);
        if (!result.Success)
        {
            Console.WriteLine($"Erro: {result.Error}");
            return;
        }

        Console.WriteLine($"Registrado {settings.DefaultCupMl} ml. Total de hoje: {result.TotalTodayMl} ml.");
        return;
    }

    if (!int.TryParse(option, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amountMl))
    {
        Console.WriteLine("Informe um valor em ml ou use --cup/--undo.");
        return;
    }

    var add = await service.AddMlAsync(amountMl);
    if (!add.Success)
    {
        Console.WriteLine($"Erro: {add.Error}");
        return;
    }

    Console.WriteLine($"Registrado {amountMl} ml. Total de hoje: {add.TotalTodayMl} ml.");
}

static async Task StartDaemonAsync()
{
    using var mutex = new Mutex(false, "Global\\HidratacaoDaemon");
    if (!mutex.WaitOne(0))
    {
        Console.WriteLine("Daemon já está em execução.");
        return;
    }

    var basePath = Environment.CurrentDirectory;
    var pidFile = Path.Combine(basePath, "daemon.pid");
    File.WriteAllText(pidFile, Environment.ProcessId.ToString(CultureInfo.InvariantCulture));

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    Console.WriteLine("Daemon iniciado. Pressione Ctrl+C para encerrar.");
    var service = CreateSettingsService();

    try
    {
        while (!cts.IsCancellationRequested)
        {
            var settings = await service.GetAsync(cts.Token);
            var now = DateTime.Now;
            var start = DateTime.Today.Add(settings.ActiveHoursStart.ToTimeSpan());
            var end = DateTime.Today.Add(settings.ActiveHoursEnd.ToTimeSpan());

            if (now < start)
            {
                await DelaySafe(start - now, cts.Token);
                continue;
            }

            if (now >= end)
            {
                await DelaySafe(start.AddDays(1) - now, cts.Token);
                continue;
            }

            var next = now.AddMinutes(settings.ReminderIntervalMinutes);
            if (next > end)
            {
                await DelaySafe(start.AddDays(1) - now, cts.Token);
                continue;
            }

            await DelaySafe(next - now, cts.Token);
            if (cts.IsCancellationRequested)
            {
                break;
            }

            EmitReminder(settings);
        }
    }
    finally
    {
        if (File.Exists(pidFile))
        {
            File.Delete(pidFile);
        }
    }
}

static void StopDaemon()
{
    var basePath = Environment.CurrentDirectory;
    var pidFile = Path.Combine(basePath, "daemon.pid");
    if (!File.Exists(pidFile))
    {
        Console.WriteLine("Daemon não está em execução.");
        return;
    }

    var content = File.ReadAllText(pidFile).Trim();
    if (!int.TryParse(content, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pid))
    {
        Console.WriteLine("PID inválido no arquivo de daemon.");
        return;
    }

    try
    {
        var process = Process.GetProcessById(pid);
        process.Kill(true);
        process.WaitForExit(5000);
        Console.WriteLine("Daemon encerrado.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Falha ao encerrar daemon: {ex.Message}");
    }
    finally
    {
        if (File.Exists(pidFile))
        {
            File.Delete(pidFile);
        }
    }
}

static SettingsService CreateSettingsService()
{
    var basePath = Environment.CurrentDirectory;
    var repository = new JsonSettingsRepository(basePath);
    return new SettingsService(repository);
}

static WaterEntryService CreateWaterEntryService()
{
    var basePath = Environment.CurrentDirectory;
    var eventRepository = new JsonWaterEventRepository(basePath);
    var summaryRepository = new JsonDailySummaryRepository(basePath);
    return new WaterEntryService(eventRepository, summaryRepository);
}

static void EmitReminder(Hidratacao.Domain.Settings settings)
{
    var remaining = settings.DailyGoalMl;
    Console.WriteLine($"[{DateTime.Now:HH:mm}] Lembrete: faltam {remaining} ml para a meta. Sugestão: {settings.DefaultCupMl} ml.");
    try
    {
        Console.Beep();
    }
    catch (PlatformNotSupportedException)
    {
    }
}

static async Task DelaySafe(TimeSpan delay, CancellationToken cancellationToken)
{
    if (delay < TimeSpan.Zero)
    {
        delay = TimeSpan.Zero;
    }

    await Task.Delay(delay, cancellationToken);
}

static void ShowHelp()
{
    Console.WriteLine("Uso:");
    Console.WriteLine("  hidratacao config --show");
    Console.WriteLine("  hidratacao config --goal 2000 --active-hours 08:00-22:00 --interval 30 --cup-size 250");
    Console.WriteLine("  hidratacao daemon start");
    Console.WriteLine("  hidratacao daemon stop");
    Console.WriteLine("  hidratacao log <ml>");
    Console.WriteLine("  hidratacao log --cup");
    Console.WriteLine("  hidratacao log --undo");
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

static void ShowDaemonHelp()
{
    Console.WriteLine("Uso do comando daemon:");
    Console.WriteLine("  start");
    Console.WriteLine("  stop");
}

static void ShowLogHelp()
{
    Console.WriteLine("Uso do comando log:");
    Console.WriteLine("  log <ml>");
    Console.WriteLine("  log --cup");
    Console.WriteLine("  log --undo");
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

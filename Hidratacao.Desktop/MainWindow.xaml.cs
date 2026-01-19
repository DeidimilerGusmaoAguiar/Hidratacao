using System.Globalization;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Hidratacao.Application;
using Hidratacao.Infrastructure;

namespace Hidratacao.Desktop;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly WaterHistoryService _historyService;
    private readonly ReminderScheduler _scheduler;
    private readonly WaterEntryService _waterEntryService;
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _summaryTimer;
    private readonly DispatcherTimer _nextReminderTimer;
    private readonly DispatcherTimer _toastTimer;
    private CancellationTokenSource? _daemonCts;
    private DateTime? _nextReminderAt;
    private Hidratacao.Domain.Settings? _lastSettings;
    private readonly Random _random = new();
    private int _lastMessageIndex = -1;
    private readonly string[] _logMessages =
    [
        "Boa. Mais um gole na conta.",
        "Olha so, a hidratacao apareceu.",
        "Copinho registrado. Milagre.",
        "Isso, finge que e atleta.",
        "Agua confirmada. Vida organizada.",
        "Um gole a mais, um drama a menos.",
        "Voce bebeu agua. O universo agradece.",
        "Mais agua. Menos desculpa.",
        "Excelente. Ainda nao virou cacto.",
        "Ta, isso conta como autocuidado.",
        "Hidratacao em dia. Quase um evento historico.",
        "Boa. Continua antes que eu reclame.",
        "So mais um e ja da pra dizer que se cuida.",
        "Gole registrado. A balanca do cosmos sorriu.",
        "Parabens, voce venceu o deserto interno.",
        "Ok, ok. Isso foi decente.",
        "Notificacao: seu corpo pediu e voce ouviu.",
        "Sim, agua. Finalmente.",
        "Mantem o ritmo, ou vai secar.",
        "Se fosse cafe, voce ja tava na terceira.",
        "Registrei. Nao some de novo.",
        "Mais agua, menos drama.",
        "Voce nao e cactus. Prove.",
        "Esse copo foi real ou imaginario?",
        "Boa. Ainda faltam uns litros, mas seguimos.",
        "Bebeu? Entao ta.",
        "Tomou agua. Pode se gabar por 5 minutos.",
        "Ok. Pelo menos isso hoje.",
        "Tem certeza que e agua? To achando que ta e comendo e vai virar uma bola e ta dizendo que ta tomando agua.",
        "Atualizado. Segue o baile."
    ];

    public MainWindow()
    {
        InitializeComponent();

        var basePath = AppContext.BaseDirectory;
        var settingsRepository = new JsonSettingsRepository(basePath);
        var summaryRepository = new JsonDailySummaryRepository(basePath);
        var eventRepository = new JsonWaterEventRepository(basePath);

        _settingsService = new SettingsService(settingsRepository);
        _historyService = new WaterHistoryService(settingsRepository, summaryRepository, eventRepository);
        _waterEntryService = new WaterEntryService(eventRepository, summaryRepository);
        _scheduler = new ReminderScheduler(_settingsService, _historyService);
        _scheduler.Reminder += (_, args) =>
        {
            ReminderText.Text = $"Ultimo lembrete: {args.OccurredAtLocal:HH:mm} - faltam {args.RemainingMl} ml";
            _ = UpdateSummaryAsync();
            _ = UpdateNextReminderAsync();
            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch
            {
            }
        };

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => ClockText.Text = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        _clockTimer.Start();

        _summaryTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _summaryTimer.Tick += async (_, _) => await UpdateSummaryAsync();
        _summaryTimer.Start();

        _nextReminderTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _nextReminderTimer.Tick += (_, _) => UpdateNextReminderCountdown();
        _nextReminderTimer.Start();

        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _toastTimer.Tick += (_, _) => HideToast();

        Opacity = OpacitySlider.Value;
        _ = UpdateSummaryAsync();
        _ = UpdateNextReminderAsync();
    }

    private async Task UpdateSummaryAsync()
    {
        var history = await _historyService.GetHistoryAsync(1);
        if (history.Count == 0)
        {
            GoalText.Text = "0 ml";
            ProgressText.Text = "0% completo";
            return;
        }

        var today = history[0];
        GoalText.Text = $"{today.TotalMl} / {today.DailyGoalMl} ml";
        ProgressText.Text = $"{today.ProgressPercent}% completo";

        var list = await _historyService.GetHistoryAsync(7);
        HistoryList.ItemsSource = list.Select(item => new HistoryRow(
            item.DateUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            $"{item.TotalMl} ml",
            item.Status)).ToList();
    }

    private async Task UpdateNextReminderAsync()
    {
        _lastSettings = await _settingsService.GetAsync();
        var lastEvent = await _historyService.GetLastEventLocalAsync();
        _nextReminderAt = ReminderScheduleCalculator.GetNextReminderAtLocal(_lastSettings, DateTime.Now, lastEvent);
        UpdateNextReminderCountdown();
    }

    private void UpdateNextReminderCountdown()
    {
        if (_nextReminderAt is null)
        {
            NextReminderText.Text = "Proximo: -";
            return;
        }

        var now = DateTime.Now;
        var remaining = _nextReminderAt.Value - now;
        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        var minutes = (int)Math.Floor(remaining.TotalMinutes);
        var seconds = remaining.Seconds;
        NextReminderText.Text = $"Proximo: {_nextReminderAt:HH:mm} (em {minutes}m {seconds}s)";
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_daemonCts is not null)
        {
            StatusText.Text = "Status: rodando";
            return;
        }

        _daemonCts = new CancellationTokenSource();
        StatusText.Text = "Status: rodando";
        await _scheduler.RunAsync(_daemonCts.Token);
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _daemonCts?.Cancel();
        _daemonCts = null;
        StatusText.Text = "Status: parado";
    }

    private async void QuickLogButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = _lastSettings ?? await _settingsService.GetAsync();
        var result = await _waterEntryService.AddCupAsync(settings.DefaultCupMl);
        if (!result.Success)
        {
            MessageBox.Show(result.Error ?? "Falha ao registrar copo.", "Hidratacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ShowToast(PickRandomMessage());
        await UpdateSummaryAsync();
        await UpdateNextReminderAsync();
    }


    private void TopmostCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Topmost = TopmostCheckBox.IsChecked == true;
    }

    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Opacity = e.NewValue;
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void ShowToast(string message)
    {
        ToastText.Text = message;
        ToastCard.Visibility = Visibility.Visible;
        _toastTimer.Stop();
        _toastTimer.Start();

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        ToastCard.BeginAnimation(OpacityProperty, fadeIn);

        var slideIn = new DoubleAnimation(-10, 0, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        ToastTranslate.BeginAnimation(TranslateTransform.YProperty, slideIn);
    }

    private void HideToast()
    {
        _toastTimer.Stop();
        var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(180));
        fadeOut.Completed += (_, _) => ToastCard.Visibility = Visibility.Collapsed;
        ToastCard.BeginAnimation(OpacityProperty, fadeOut);

        var slideOut = new DoubleAnimation(-10, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        ToastTranslate.BeginAnimation(TranslateTransform.YProperty, slideOut);
    }

    private void ToastCloseButton_Click(object sender, RoutedEventArgs e)
    {
        HideToast();
    }

    private string PickRandomMessage()
    {
        if (_logMessages.Length == 0)
        {
            return string.Empty;
        }

        int index;
        do
        {
            index = _random.Next(_logMessages.Length);
        } while (_logMessages.Length > 1 && index == _lastMessageIndex);

        _lastMessageIndex = index;
        return _logMessages[index];
    }

    private sealed record HistoryRow(string Date, string Total, string Status);
}

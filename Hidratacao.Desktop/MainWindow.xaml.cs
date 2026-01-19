using System.Globalization;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Hidratacao.Application;
using Hidratacao.Infrastructure;

namespace Hidratacao.Desktop;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly WaterHistoryService _historyService;
    private readonly ReminderScheduler _scheduler;
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _summaryTimer;
    private CancellationTokenSource? _daemonCts;

    public MainWindow()
    {
        InitializeComponent();

        var basePath = AppContext.BaseDirectory;
        var settingsRepository = new JsonSettingsRepository(basePath);
        var summaryRepository = new JsonDailySummaryRepository(basePath);
        var eventRepository = new JsonWaterEventRepository(basePath);

        _settingsService = new SettingsService(settingsRepository);
        _historyService = new WaterHistoryService(settingsRepository, summaryRepository, eventRepository);
        _scheduler = new ReminderScheduler(_settingsService, _historyService);
        _scheduler.Reminder += (_, args) =>
        {
            ReminderText.Text = $"Ultimo lembrete: {args.OccurredAtLocal:HH:mm} - faltam {args.RemainingMl} ml";
            _ = UpdateSummaryAsync();
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

        Opacity = OpacitySlider.Value;
        _ = UpdateSummaryAsync();
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

}

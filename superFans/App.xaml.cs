using System.Windows;
using superFans.Services;
using superFans.ViewModels;

namespace superFans;

public partial class App : Application
{
    private readonly IHardwareService _hardware = new HardwareService();
    private readonly SafetyGuardService _safety = new();
    private readonly IFanControlService _fanControl;
    private readonly MonitoringService _monitoring = new();
    private MainViewModel? _mainVm;

    public App()
    {
        _fanControl = new FanControlService(_safety);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mainVm = new MainViewModel(_hardware, _fanControl, _monitoring);

        var window = new MainWindow
        {
            DataContext = _mainVm
        };

        window.Closed += OnMainWindowClosed;
        DispatcherUnhandledException += OnUnhandledException;

        _mainVm.Initialize();
        window.Show();
    }

    private async void OnMainWindowClosed(object? sender, EventArgs e)
    {
        if (_mainVm != null)
            await _mainVm.ShutdownAsync();
    }

    private void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            if (_mainVm != null)
            {
                foreach (var fan in _mainVm.Fans)
                {
                    if (fan.IsControllable)
                        _fanControl.ReleaseControl(fan.Fan);
                }
            }

            _hardware.Shutdown();
        }
        catch
        {
            // Best-effort safety release
        }

        MessageBox.Show($"程序发生异常，已安全释放所有风扇控制。\n\n{e.Exception.Message}",
            "SuperFans — 错误", MessageBoxButton.OK, MessageBoxImage.Error);

        e.Handled = true;
        Shutdown();
    }
}
